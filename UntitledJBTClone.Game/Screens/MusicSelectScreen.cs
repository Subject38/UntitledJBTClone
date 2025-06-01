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
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osuTK;
using osuTK.Graphics;
using UntitledJBTClone.Game.Components;
using UntitledJBTClone.Game.Models;
using UntitledJBTClone.Game.Services;

namespace UntitledJBTClone.Game.Screens
{
    public partial class MusicSelectScreen : Screen
    {
        private Profile currentProfile;
        private SongService songService;
        private List<SongInfo> songs;
        private int selectedSongIndex = 0;
        private int songPageOffset = 0;
        private const int SongsPerPage = 12;
        private Container buttonContainer;
        private SpriteText songTitleDisplay;
        private SpriteText songArtistDisplay;
        private SpriteText ratingDisplay;
        private SpriteText scoreDisplay;
        private Container jacketContainer;
        private AudioManager audioManager;
        private Track currentPreviewTrack;
        private bool isPreviewPlaying = false;
        private GameHost host;

        public MusicSelectScreen(Profile profile)
        {
            currentProfile = profile;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, GameHost gameHost)
        {
            audioManager = audio;
            host = gameHost;
            songService = new SongService();
            songs = songService.GetAllSongs();

            if (songs.Count > 0)
                selectedSongIndex = 0;

            InternalChildren = new Drawable[]
            {
                // Background
                new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                },

                // Main container
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        // Top section with music info
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 300,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Children = new Drawable[]
                            {
                                // Blue background for music select header
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Blue,
                                },

                                // Music info layout
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(20),
                                    Children = new Drawable[]
                                    {
                                        // Left side - Music Select title and song info
                                        new FillFlowContainer
                                        {
                                            Direction = FillDirection.Vertical,
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            AutoSizeAxes = Axes.Both,
                                            Spacing = new Vector2(0, 10),
                                            Children = new Drawable[]
                                            {
                                                new SpriteText
                                                {
                                                    Text = "Music\nSelect",
                                                    Font = FontUsage.Default.With(size: 36),
                                                    Colour = Color4.White,
                                                },
                                                songTitleDisplay = new SpriteText
                                                {
                                                    Text = GetCurrentSongTitle(),
                                                    Font = FontUsage.Default.With(size: 20),
                                                    Colour = Color4.White,
                                                },
                                                songArtistDisplay = new SpriteText
                                                {
                                                    Text = GetCurrentSongArtist(),
                                                    Font = FontUsage.Default.With(size: 16),
                                                    Colour = Color4.LightGray,
                                                },
                                                ratingDisplay = new SpriteText
                                                {
                                                    Text = GetCurrentSongRating(),
                                                    Font = FontUsage.Default.With(size: 16),
                                                    Colour = Color4.White,
                                                }
                                            }
                                        },

                                        // Center - Song jacket
                                        jacketContainer = new Container
                                        {
                                            Size = new Vector2(200, 200),
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Masking = true,
                                            CornerRadius = 10,
                                            Children = new Drawable[]
                                            {
                                                new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Colour = Color4.Gray,
                                                }
                                            }
                                        },

                                        // Right side - Score display
                                        scoreDisplay = new SpriteText
                                        {
                                            Text = "Score: XXXXXXX",
                                            Font = FontUsage.Default.With(size: 16),
                                            Colour = Color4.White,
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                        }
                                    }
                                }
                            }
                        },

                        // Gray separator area
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 50,
                            Y = 300,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Gray,
                            }
                        },

                        // Song list area (placeholder)
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 200,
                            Y = 350,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.DarkGray,
                            }
                        },

                        // Button grid at bottom
                        buttonContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = new ButtonGrid(CreateMusicSelectButton)
                            {
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                Y = -40,
                            }
                        }
                    }
                }
            };

            // Update initial display
            UpdateSongDisplay();

            // Start preview for initial song
            if (songs.Count > 0)
                StartPreview();
        }

        private string GetCurrentSongTitle()
        {
            if (songs.Count == 0) return "No Songs";
            if (selectedSongIndex >= songs.Count) return "No Song Selected";

            var song = songs[selectedSongIndex];
            return !string.IsNullOrEmpty(song.MemonData.Metadata.Title)
                ? song.MemonData.Metadata.Title
                : "Unknown Title";
        }

        private string GetCurrentSongArtist()
        {
            if (songs.Count == 0) return "";
            if (selectedSongIndex >= songs.Count) return "";

            var song = songs[selectedSongIndex];
            return !string.IsNullOrEmpty(song.MemonData.Metadata.Artist)
                ? song.MemonData.Metadata.Artist
                : "Unknown Artist";
        }

        private string GetCurrentSongRating()
        {
            if (songs.Count == 0) return "Rating: --";
            if (selectedSongIndex >= songs.Count) return "Rating: --";

            var song = songs[selectedSongIndex];
            var charts = song.MemonData.Data;

            if (charts.Count > 0)
            {
                var firstChart = charts.Values.First();
                return $"Rating: {firstChart.Level:F1}";
            }

            return "Rating: --";
        }

        private GameButton CreateMusicSelectButton(int number)
        {
            // Song buttons (1-12)
            if (number >= 1 && number <= 12)
            {
                int songIndex = songPageOffset + (number - 1);
                if (songIndex < songs.Count)
                {
                    var song = songs[songIndex];
                    var songContent = CreateSongButtonContent(song);
                    var songButton = new GameButton(songContent);
                    songButton.Action = () => SelectSong(songIndex);
                    return songButton;
                }
                else
                {
                    return new GameButton("");
                }
            }

            switch (number)
            {
                case 13: // Left arrow
                    var leftButton = new GameButton("◄");
                    leftButton.Action = () => ScrollSongs(-SongsPerPage);
                    return leftButton;

                case 14: // Right arrow  
                    var rightButton = new GameButton("►");
                    rightButton.Action = () => ScrollSongs(SongsPerPage);
                    return rightButton;

                case 15: // Settings
                    var settingsButton = new GameButton("Settings");
                    return settingsButton;

                case 16: // Play
                    var playButton = new GameButton("Play");
                    playButton.Action = () => StartGame();
                    return playButton;

                default:
                    return new GameButton("");
            }
        }

        private Drawable CreateSongButtonContent(SongInfo song)
        {
            return new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    // Title at the top
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 25,
                        Child = new SpriteText
                        {
                            Text = !string.IsNullOrEmpty(song.MemonData.Metadata.Title)
                                ? song.MemonData.Metadata.Title
                                : "Unknown",
                            Font = FontUsage.Default.With(size: 12),
                            Colour = Color4.White,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Truncate = true,
                        }
                    },
                    // Jacket image below
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Height = 0.75f, // Use 75% of button height for jacket
                        Y = -25, // Offset by title height
                        Masking = true,
                        Child = CreateJacketSprite(song)
                    }
                }
            };
        }

        private Drawable CreateJacketSprite(SongInfo song)
        {
            Logger.Log($"CreateJacketSprite called for song: {song?.MemonData?.Metadata?.Title ?? "Unknown"}", LoggingTarget.Runtime, LogLevel.Debug);
            Logger.Log($"Jacket path: {song?.JacketPath ?? "null"}", LoggingTarget.Runtime, LogLevel.Debug);

            if (!string.IsNullOrEmpty(song.JacketPath) && File.Exists(song.JacketPath))
            {
                Logger.Log($"Attempting to load button jacket: {song.JacketPath}", LoggingTarget.Runtime, LogLevel.Debug);
                try
                {
                    // Use direct StorageBackedResourceStore approach that we know works
                    var folderStorage = new NativeStorage(Path.GetDirectoryName(song.JacketPath));
                    var resourceStore = new StorageBackedResourceStore(folderStorage);
                    var textureLoaderStore = host.CreateTextureLoaderStore(resourceStore);
                    var textureStore = new TextureStore(host.Renderer, textureLoaderStore);

                    var fileName = Path.GetFileName(song.JacketPath);
                    Logger.Log($"Loading button texture file: {fileName}", LoggingTarget.Runtime, LogLevel.Debug);
                    var texture = textureStore.Get(fileName);

                    if (texture != null)
                    {
                        Logger.Log($"Button texture loaded successfully: {fileName} ({texture.Width}x{texture.Height})", LoggingTarget.Runtime, LogLevel.Debug);
                        // Use the same approach that worked for the main jacket
                        return new Sprite
                        {
                            Texture = texture,
                            Size = new Vector2(texture.Width, texture.Height), // Use native texture size
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Scale = new Vector2(0.3f), // Scale down to fit in button (smaller than main jacket)
                        };
                    }
                    else
                    {
                        Logger.Log($"Button texture was null for {fileName}", LoggingTarget.Runtime, LogLevel.Debug);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to load button jacket {song.JacketPath}: {ex.Message}", LoggingTarget.Runtime, LogLevel.Debug);
                }
            }
            else
            {
                Logger.Log($"Button jacket path invalid or file doesn't exist: {song?.JacketPath ?? "null"}", LoggingTarget.Runtime, LogLevel.Debug);
            }

            // Show placeholder if no jacket or failed to load
            Logger.Log("Returning gray placeholder for button", LoggingTarget.Runtime, LogLevel.Debug);
            return new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.Gray,
            };
        }

        private void SelectSong(int songIndex)
        {
            selectedSongIndex = songIndex;
            UpdateSongDisplay();
            StartPreview();
        }

        private void ScrollSongs(int direction)
        {
            int newOffset = songPageOffset + direction;
            if (newOffset >= 0 && newOffset < songs.Count)
            {
                songPageOffset = newOffset;
                RefreshButtonGrid();
            }
        }

        private void UpdateSongDisplay()
        {
            songTitleDisplay.Text = GetCurrentSongTitle();
            songArtistDisplay.Text = GetCurrentSongArtist();
            ratingDisplay.Text = GetCurrentSongRating();
            UpdateJacketDisplay();
        }

        private void UpdateJacketDisplay()
        {
            jacketContainer.Clear();

            if (songs.Count == 0 || selectedSongIndex >= songs.Count)
            {
                // Show placeholder
                jacketContainer.Add(new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Gray,
                });
                return;
            }

            var currentSong = songs[selectedSongIndex];

            if (!string.IsNullOrEmpty(currentSong.JacketPath) && File.Exists(currentSong.JacketPath))
            {
                Logger.Log($"Attempting to load main jacket: {currentSong.JacketPath}", LoggingTarget.Runtime, LogLevel.Debug);
                try
                {
                    // Use direct StorageBackedResourceStore approach that we know works
                    var folderStorage = new NativeStorage(Path.GetDirectoryName(currentSong.JacketPath));
                    var resourceStore = new StorageBackedResourceStore(folderStorage);
                    var textureLoaderStore = host.CreateTextureLoaderStore(resourceStore);
                    var textureStore = new TextureStore(host.Renderer, textureLoaderStore);

                    var fileName = Path.GetFileName(currentSong.JacketPath);
                    Logger.Log($"Loading texture file: {fileName}", LoggingTarget.Runtime, LogLevel.Debug);
                    var texture = textureStore.Get(fileName);

                    if (texture != null)
                    {
                        Logger.Log($"Loaded texture: {fileName}", LoggingTarget.Runtime, LogLevel.Debug);
                        Logger.Log($"Texture size: {texture.Width}x{texture.Height}", LoggingTarget.Runtime, LogLevel.Debug);
                        Logger.Log($"Texture available: {texture.Available}", LoggingTarget.Runtime, LogLevel.Debug);
                        Logger.Log($"Texture type: {texture.GetType().Name}", LoggingTarget.Runtime, LogLevel.Debug);

                        // Try displaying at native size first
                        jacketContainer.Add(new Sprite
                        {
                            Texture = texture,
                            Size = new Vector2(texture.Width, texture.Height), // Use native texture size
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Scale = new Vector2(0.625f), // Scale down to fit in 200x200 container (200/320 = 0.625)
                        });
                        return;
                    }
                    else
                    {
                        Logger.Log($"Texture was null for {fileName}", LoggingTarget.Runtime, LogLevel.Debug);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load jacket {currentSong.JacketPath}: {ex.Message}");
                }
            }


            // Show placeholder if no jacket or failed to load
            jacketContainer.Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.Gray,
            });
        }

        private void RefreshButtonGrid()
        {
            buttonContainer.Clear();
            buttonContainer.Add(new ButtonGrid(CreateMusicSelectButton)
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                Y = -40,
            });
        }

        private void StartPreview()
        {
            StopPreview();

            if (songs.Count == 0 || selectedSongIndex >= songs.Count)
                return;

            var currentSong = songs[selectedSongIndex];
            if (string.IsNullOrEmpty(currentSong.AudioPath) || !File.Exists(currentSong.AudioPath))
                return;

            try
            {
                // Create a storage backed resource store for the song folder
                var folderStorage = new NativeStorage(Path.GetDirectoryName(currentSong.AudioPath));
                var resourceStore = new StorageBackedResourceStore(folderStorage);
                var trackStore = audioManager.GetTrackStore(resourceStore);

                var fileName = Path.GetFileName(currentSong.AudioPath);
                currentPreviewTrack = trackStore.Get(fileName);

                if (currentPreviewTrack != null)
                {
                    var preview = currentSong.MemonData.Metadata.Preview;

                    if (!string.IsNullOrEmpty(preview.File))
                    {
                        // Use separate preview file if specified
                        var previewFileName = preview.File;
                        if (File.Exists(Path.Combine(currentSong.FolderPath, previewFileName)))
                        {
                            currentPreviewTrack = trackStore.Get(previewFileName);
                        }
                    }

                    if (currentPreviewTrack != null)
                    {
                        // Set up preview timing
                        double startTime = preview.Start > 0 ? preview.Start : 0;
                        double duration = preview.Duration > 0 ? preview.Duration : 7; // Default 7 seconds

                        currentPreviewTrack.Seek(startTime * 1000); // Convert to milliseconds
                        currentPreviewTrack.Start();
                        isPreviewPlaying = true;

                        // Schedule stop after duration, then restart for loop
                        Scheduler.AddDelayed(() =>
                        {
                            if (isPreviewPlaying && currentPreviewTrack != null)
                            {
                                currentPreviewTrack.Seek(startTime * 1000);
                                currentPreviewTrack.Start();

                                // Stop after second loop
                                Scheduler.AddDelayed(() =>
                                {
                                    StopPreview();
                                }, duration * 1000);
                            }
                        }, duration * 1000);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start preview for {currentSong.AudioPath}: {ex.Message}");
            }
        }

        private void StopPreview()
        {
            if (currentPreviewTrack != null)
            {
                currentPreviewTrack.Stop();
                currentPreviewTrack.Dispose();
                currentPreviewTrack = null;
            }
            isPreviewPlaying = false;
        }

        private void StartGame()
        {
            StopPreview(); // Stop preview when starting game

            if (songs.Count == 0 || selectedSongIndex >= songs.Count)
                return;

            var selectedSong = songs[selectedSongIndex];

            // For now, just show a placeholder gameplay screen
            this.Push(new GameplayScreen(currentProfile, selectedSong));
        }

        protected override void Dispose(bool isDisposing)
        {
            StopPreview();
            base.Dispose(isDisposing);
        }
    }
}