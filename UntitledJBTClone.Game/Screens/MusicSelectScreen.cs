using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osuTK;
using osuTK.Graphics;
using UntitledJBTClone.Game.Components;
using UntitledJBTClone.Game.Models;

namespace UntitledJBTClone.Game.Screens
{
    public partial class MusicSelectScreen : Screen
    {
        private Profile currentProfile;

        public MusicSelectScreen(Profile profile)
        {
            currentProfile = profile;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
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
                                        // Left side - Music Select title and rating
                                        new FillFlowContainer
                                        {
                                            Direction = FillDirection.Vertical,
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            AutoSizeAxes = Axes.Both,
                                            Spacing = new Vector2(0, 20),
                                            Children = new Drawable[]
                                            {
                                                new SpriteText
                                                {
                                                    Text = "Music\nSelect",
                                                    Font = FontUsage.Default.With(size: 36),
                                                    Colour = Color4.White,
                                                },
                                                new SpriteText
                                                {
                                                    Text = "Rating: X.X",
                                                    Font = FontUsage.Default.With(size: 16),
                                                    Colour = Color4.White,
                                                }
                                            }
                                        },

                                        // Center - Song jacket placeholder
                                        new Container
                                        {
                                            Size = new Vector2(200, 200),
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Children = new Drawable[]
                                            {
                                                new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Colour = Color4.Gray,
                                                },
                                                new SpriteText
                                                {
                                                    Text = "Song\nJacket",
                                                    Font = FontUsage.Default.With(size: 24),
                                                    Colour = Color4.White,
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                }
                                            }
                                        },

                                        // Right side - Score display
                                        new SpriteText
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
                        new ButtonGrid(CreateMusicSelectButton)
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Y = -40,
                        }
                    }
                }
            };
        }

        private GameButton CreateMusicSelectButton(int number)
        {
            // First button shows selected song info
            if (number == 1)
            {
                var songButton = new GameButton("Example Song");
                return songButton;
            }

            // Other song buttons (2-12) - empty for now
            if (number >= 2 && number <= 12)
            {
                return new GameButton("");
            }

            switch (number)
            {
                case 13: // Left arrow
                    var leftButton = new GameButton("◄");
                    return leftButton;

                case 14: // Right arrow  
                    var rightButton = new GameButton("►");
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

        private void StartGame()
        {
            // For now, just go back to profile select
            // In the future, this would start the actual game
            this.Exit();
        }
    }
}