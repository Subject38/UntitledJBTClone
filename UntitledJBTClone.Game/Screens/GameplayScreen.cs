using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Timing;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;
using UntitledJBTClone.Game.Components;
using UntitledJBTClone.Game.Models;

namespace UntitledJBTClone.Game.Screens
{
    public partial class GameplayScreen : Screen
    {
        private Profile currentProfile;
        private SongInfo currentSong;
        private MemonChart selectedChart;
        private Track audioTrack;
        private AudioManager audioManager;
        private GameHost host;

        // Timing and gameplay
        private double songTime = 0;
        private double chartOffset = 0;
        private List<MemonBpm> bpmEvents;
        private List<PlayableNote> playableNotes;
        private int currentBpmIndex = 0;
        private double currentBpm = 120;

        // Visual elements
        private Container gameplayArea;
        private GameplayButton[] gameplayButtons = new GameplayButton[16];
        private SpriteText scoreDisplay;
        private SpriteText timeDisplay;
        private SpriteText songDisplay;
        private SpriteText debugDisplay;
        private Container topUI;
        private Container bottomControls;

        // Game state
        private int score = 0;
        private bool isPlaying = false;
        private bool gameStarted = false;
        private double gameStartTime = 0;

        // Input timing
        private const double HitWindow = 0.1; // Tighter hit window for better precision
        private const double NoteAppearTime = 1.0; // Faster animation - notes appear 1 second before hit

        public GameplayScreen(Profile profile, SongInfo song)
        {
            currentProfile = profile;
            currentSong = song;

            // Select the first available chart (or could add difficulty selection later)
            if (song.MemonData.Data.Count > 0)
            {
                selectedChart = song.MemonData.Data.Values.First();
            }
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, GameHost gameHost)
        {
            audioManager = audio;
            host = gameHost;

            // Load audio track using the same pattern as MusicSelectScreen
            LoadAudioTrack();

            // Prepare chart data
            PrepareChartData();

            CreateLayout();
        }

        private void LoadAudioTrack()
        {
            try
            {
                // Create storage backed resource store for the song folder
                var folderStorage = new NativeStorage(Path.GetDirectoryName(currentSong.AudioPath));
                var resourceStore = new StorageBackedResourceStore(folderStorage);
                var trackStore = audioManager.GetTrackStore(resourceStore);

                var fileName = Path.GetFileName(currentSong.AudioPath);
                audioTrack = trackStore.Get(fileName);
            }
            catch (Exception)
            {
                // Audio loading failed - continue without audio
            }
        }

        private void CreateLayout()
        {
            InternalChildren = new Drawable[]
            {
                // Background
                new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                },
                
                // Top UI
                topUI = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 80,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.DarkBlue,
                        },
                        new FillFlowContainer
                        {
                            Direction = FillDirection.Horizontal,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Spacing = new Vector2(50, 0),
                            Children = new Drawable[]
                            {
                                scoreDisplay = new SpriteText
                                {
                                    Text = "Score: 0",
                                    Font = FontUsage.Default.With(size: 24),
                                    Colour = Color4.White,
                                },
                                songDisplay = new SpriteText
                                {
                                    Text = currentSong.MemonData.Metadata.Title,
                                    Font = FontUsage.Default.With(size: 20),
                                    Colour = Color4.White,
                                },
                                timeDisplay = new SpriteText
                                {
                                    Text = "0:00",
                                    Font = FontUsage.Default.With(size: 18),
                                    Colour = Color4.White,
                                },
                                // Add debug status display
                                debugDisplay = new SpriteText
                                {
                                    Text = "Gameplay Screen",
                                    Font = FontUsage.Default.With(size: 16),
                                    Colour = Color4.White,
                                }
                            }
                        }
                    }
                },
                
                // Main gameplay area (4x4 grid)
                gameplayArea = new Container
                {
                    Size = new Vector2(480, 480),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },

                // Bottom controls
                bottomControls = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 60,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.DarkGray,
                        },
                        new FillFlowContainer
                        {
                            Direction = FillDirection.Horizontal,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Spacing = new Vector2(20, 0),
                            Children = new Drawable[]
                            {
                                CreateControlButton("Start", StartGame),
                                CreateControlButton("Pause", TogglePlayback),
                                CreateControlButton("Stop", StopGame),
                                CreateControlButton("Restart", RestartGame),
                                CreateControlButton("Back", () => this.Exit())
                            }
                        }
                    }
                }
            };

            CreateGameplayGrid();
        }

        private GameButton CreateControlButton(string text, Action action)
        {
            var button = new GameButton(text);
            button.Action = action;
            return button;
        }

        private void CreateGameplayGrid()
        {
            gameplayArea.Clear();

            for (int i = 0; i < 16; i++)
            {
                int row = i / 4;
                int col = i % 4;
                int buttonIndex = i; // Capture the loop variable to avoid closure issue

                var gameplayButton = new GameplayButton(i)
                {
                    Size = new Vector2(110, 110),
                    Position = new Vector2(col * 120, row * 120),
                };

                gameplayButton.OnPressed += () => HitButton(buttonIndex); // Use captured variable
                gameplayButtons[i] = gameplayButton;
                gameplayArea.Add(gameplayButton);
            }
        }

        private void PrepareChartData()
        {
            if (selectedChart == null) return;

            // Get timing info (chart-specific or default)
            var timing = selectedChart.Timing ?? currentSong.MemonData.Timing;
            chartOffset = timing.Offset;
            bpmEvents = timing.Bpms.OrderBy(b => b.Beat).ToList();

            // Convert notes to playable format
            playableNotes = new List<PlayableNote>();
            foreach (var note in selectedChart.Notes)
            {
                var playableNote = new PlayableNote
                {
                    Position = note.N,
                    BeatTime = note.T,
                    Duration = note.L,
                    IsLongNote = note.IsLongNote,
                    TailPosition = note.P,
                    TimeInSeconds = BeatToSeconds(note.T, timing),
                    IsHit = false,
                    IsActive = false
                };
                playableNotes.Add(playableNote);
            }

            // Sort notes by time
            playableNotes = playableNotes.OrderBy(n => n.TimeInSeconds).ToList();
        }

        private double BeatToSeconds(double beatTime, MemonTiming timing)
        {
            // Convert memon timing units to actual beats using resolution
            double actualBeats = beatTime / selectedChart.Resolution;

            double seconds = timing.Offset;
            double currentBeat = 0;
            double currentBpm = 120;

            foreach (var bpmEvent in timing.Bpms.OrderBy(b => b.Beat))
            {
                // BPM events are already in actual beats, don't convert by resolution
                double bpmEventBeats = bpmEvent.Beat;

                if (bpmEventBeats > actualBeats) break;

                // Add time for the previous BPM section
                if (currentBeat < bpmEventBeats)
                {
                    double beatsInSection = Math.Min(bpmEventBeats - currentBeat, actualBeats - currentBeat);
                    seconds += (beatsInSection * 60.0) / currentBpm;
                    currentBeat += beatsInSection;
                }

                currentBpm = bpmEvent.Bpm;
            }

            // Add remaining time at current BPM
            if (currentBeat < actualBeats)
            {
                double remainingBeats = actualBeats - currentBeat;
                seconds += (remainingBeats * 60.0) / currentBpm;
            }

            return seconds;
        }

        private void StartGame()
        {
            if (audioTrack == null) return;

            audioTrack.Seek(0);
            audioTrack.Start();
            isPlaying = true;
            gameStarted = true;
            gameStartTime = Clock.CurrentTime / 1000.0;

            // Reset game state
            foreach (var note in playableNotes)
            {
                note.IsHit = false;
                note.IsActive = false;
            }

            score = 0;
            songTime = 0;
            currentBpmIndex = 0;
            scoreDisplay.Text = "Score: 0";
        }

        private void TogglePlayback()
        {
            if (audioTrack == null || !gameStarted) return;

            if (isPlaying)
            {
                audioTrack.Stop();
                isPlaying = false;
            }
            else
            {
                audioTrack.Start();
                isPlaying = true;
            }
        }

        private void StopGame()
        {
            if (audioTrack == null) return;

            audioTrack.Stop();
            audioTrack.Seek(0);
            isPlaying = false;
            gameStarted = false;
            songTime = 0;

            // Clear all note visuals
            foreach (var button in gameplayButtons)
            {
                if (button != null)
                {
                    button.ClearNote();
                }
            }
        }

        private void RestartGame()
        {
            StopGame();
            StartGame();
        }

        private void HitButton(int position)
        {
            if (!gameStarted || !isPlaying) return;

            if (position < 0 || position >= 16 || gameplayButtons[position] == null)
                return;

            // Find notes at this position within hit window
            var hitableNotes = playableNotes.Where(note =>
                !note.IsHit &&
                note.IsActive &&
                note.Position == position &&
                Math.Abs(songTime - note.TimeInSeconds) <= HitWindow).ToList();

            if (hitableNotes.Any())
            {
                var note = hitableNotes.First();
                HitNote(note);
            }
            else
            {
                // Miss - flash red
                gameplayButtons[position].FlashMiss();
            }

            // Always provide button feedback
            gameplayButtons[position].PressButton();
        }

        private void HitNote(PlayableNote note)
        {
            note.IsHit = true;
            note.IsActive = false;

            // Calculate score based on timing accuracy
            double timeDiff = Math.Abs(songTime - note.TimeInSeconds);
            int noteScore = 100;
            if (timeDiff < 0.05) noteScore = 300; // Perfect
            else if (timeDiff < 0.1) noteScore = 200; // Great  
            else if (timeDiff < 0.15) noteScore = 100; // Good

            score += noteScore;
            scoreDisplay.Text = $"Score: {score}";

            // Visual feedback
            if (note.Position >= 0 && note.Position < 16 && gameplayButtons[note.Position] != null)
            {
                gameplayButtons[note.Position].FlashHit();
                gameplayButtons[note.Position].ClearNote();
            }
        }

        protected override void Update()
        {
            base.Update();

            if (gameStarted && isPlaying && audioTrack != null)
            {
                songTime = audioTrack.CurrentTime / 1000.0;
                UpdateGameplay();
                UpdateUI();
            }
        }

        private void UpdateGameplay()
        {
            // Update BPM if needed
            while (currentBpmIndex < bpmEvents.Count - 1)
            {
                var nextBpmEvent = bpmEvents[currentBpmIndex + 1];
                // BPM events are already in actual beats, so convert them directly
                double nextBpmTimeSeconds = chartOffset + (nextBpmEvent.Beat * 60.0 / currentBpm);

                if (songTime >= nextBpmTimeSeconds)
                {
                    currentBpmIndex++;
                    currentBpm = bpmEvents[currentBpmIndex].Bpm;
                }
                else
                {
                    break;
                }
            }

            // Process notes
            int activeNotes = 0;
            foreach (var note in playableNotes.Where(n => !n.IsHit))
            {
                double noteTime = note.TimeInSeconds;
                double timeDiff = songTime - noteTime;

                // Note should appear NoteAppearTime seconds before hit time
                if (timeDiff >= -NoteAppearTime && timeDiff <= HitWindow && !note.IsActive)
                {
                    note.IsActive = true;
                    ShowNote(note);
                }

                // Update note animation if active
                if (note.IsActive && note.Position >= 0 && note.Position < 16)
                {
                    UpdateNoteAnimation(note, timeDiff);
                    activeNotes++;
                }

                // Auto-miss notes that are too late
                if (timeDiff > HitWindow && note.IsActive)
                {
                    note.IsHit = true;
                    note.IsActive = false;
                    if (note.Position >= 0 && note.Position < 16 && gameplayButtons[note.Position] != null)
                    {
                        gameplayButtons[note.Position].ClearNote();
                    }
                }
            }
        }

        private void ShowNote(PlayableNote note)
        {
            if (note.Position >= 0 && note.Position < 16 && gameplayButtons[note.Position] != null)
            {
                gameplayButtons[note.Position].ShowNote(note.IsLongNote);
            }
        }

        private void UpdateNoteAnimation(PlayableNote note, double timeDiff)
        {
            if (note.Position >= 0 && note.Position < 16 && gameplayButtons[note.Position] != null)
            {
                // Calculate shrinking animation progress - more dramatic scaling
                double progress = (timeDiff + NoteAppearTime) / NoteAppearTime;
                progress = Math.Max(0, Math.Min(1, progress));

                gameplayButtons[note.Position].UpdateNoteAnimation(progress);
            }
        }

        private void UpdateUI()
        {
            int minutes = (int)(songTime / 60);
            int seconds = (int)(songTime % 60);
            timeDisplay.Text = $"{minutes}:{seconds:D2}";

            // Show useful game info instead of debug info
            int totalNotes = playableNotes?.Count ?? 0;
            int hitNotes = playableNotes?.Count(n => n.IsHit) ?? 0;
            int remainingNotes = totalNotes - hitNotes;

            debugDisplay.Text = $"Notes: {hitNotes}/{totalNotes} remaining: {remainingNotes}";
            debugDisplay.Colour = Color4.White;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            // Map keyboard keys to buttons for testing
            var keyMap = new Dictionary<Key, int>
            {
                { Key.Q, 0 }, { Key.W, 1 }, { Key.E, 2 }, { Key.R, 3 },
                { Key.A, 4 }, { Key.S, 5 }, { Key.D, 6 }, { Key.F, 7 },
                { Key.Z, 8 }, { Key.X, 9 }, { Key.C, 10 }, { Key.V, 11 },
                { Key.Number1, 12 }, { Key.Number2, 13 }, { Key.Number3, 14 }, { Key.Number4, 15 }
            };

            if (keyMap.TryGetValue(e.Key, out int position))
            {
                HitButton(position);
                return true;
            }

            // Control keys
            switch (e.Key)
            {
                case Key.Space:
                    if (!gameStarted) StartGame();
                    else TogglePlayback();
                    return true;
                case Key.Escape:
                    this.Exit();
                    return true;
            }

            return base.OnKeyDown(e);
        }

        protected override void Dispose(bool isDisposing)
        {
            audioTrack?.Dispose();
            base.Dispose(isDisposing);
        }
    }

    public class PlayableNote
    {
        public int Position { get; set; }
        public double BeatTime { get; set; }
        public double Duration { get; set; }
        public bool IsLongNote { get; set; }
        public int TailPosition { get; set; }
        public double TimeInSeconds { get; set; }
        public bool IsHit { get; set; }
        public bool IsActive { get; set; }
    }

    public partial class GameplayButton : Container
    {
        private int position;
        private Box background;
        private Container border;
        private Container noteContainer;
        private Box noteVisual;
        private SpriteText positionText;

        public Action OnPressed;

        public GameplayButton(int pos)
        {
            position = pos;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                // Background
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.DarkGray,
                },
                // Border
                border = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderThickness = 3,
                    BorderColour = Color4.White,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                    }
                },
                // Position number
                positionText = new SpriteText
                {
                    Text = (position + 1).ToString(),
                    Font = FontUsage.Default.With(size: 16),
                    Colour = Color4.LightGray,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Margin = new MarginPadding(5),
                },
                // Note container
                noteContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    Child = noteVisual = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Cyan,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                }
            };
        }

        protected override bool OnClick(ClickEvent e)
        {
            OnPressed?.Invoke();
            return true;
        }

        public void PressButton()
        {
            background.FlashColour(Color4.White, 100);
        }

        public void FlashHit()
        {
            background.FlashColour(Color4.Green, 200);
        }

        public void FlashMiss()
        {
            background.FlashColour(Color4.Red, 200);
        }

        public void ShowNote(bool isLongNote)
        {
            noteVisual.Colour = isLongNote ? Color4.Yellow : Color4.Cyan;
            noteContainer.FadeIn(100);
        }

        public void UpdateNoteAnimation(double progress)
        {
            // Shrink the note as it approaches hit time
            double scale = 1.0 - (progress * 0.8);
            noteVisual.Scale = new Vector2((float)scale);
        }

        public void ClearNote()
        {
            noteContainer.FadeOut(100);
        }
    }
}