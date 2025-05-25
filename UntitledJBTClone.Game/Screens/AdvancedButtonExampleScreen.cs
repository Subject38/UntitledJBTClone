using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osuTK;
using osuTK.Graphics;
using UntitledJBTClone.Game.Components;

namespace UntitledJBTClone.Game.Screens
{
    public partial class AdvancedButtonExampleScreen : Screen
    {
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
                // Title
                new SpriteText
                {
                    Text = "Advanced Button Examples",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = FontUsage.Default.With(size: 48),
                    Colour = Color4.White,
                    Y = 100
                },
                // Animated button grid with back button
                new ButtonGrid(CreateAnimatedButtonWithBack)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Y = -40,
                }
            };
        }

        private GameButton CreateAnimatedButtonWithBack(int number)
        {
            // Button 1 will be the back button
            if (number == 1)
            {
                var backButton = new GameButton("← Back");
                backButton.Action = () => this.Exit();
                return backButton;
            }

            // Create different types of animated content based on button number
            Drawable content = number switch
            {
                <= 4 => CreatePulsingCircle(GetColorForButton(number)),
                <= 8 => CreateRotatingSquare(GetColorForButton(number)),
                <= 12 => CreateTextWithIcon(number),
                _ => CreateComplexAnimatedContent(number)
            };

            return new GameButton(content);
        }

        private static Drawable CreatePulsingCircle(Color4 color)
        {
            return new PulsingCircle
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(60, 60),
                Colour = color,
            };
        }

        private static Drawable CreateRotatingSquare(Color4 color)
        {
            return new RotatingSquare
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(50, 50),
                Colour = color,
            };
        }

        private static Drawable CreateTextWithIcon(int number)
        {
            var container = new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Both,
                Spacing = new Vector2(0, 5),
                Children = new Drawable[]
                {
                    new Circle
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Size = new Vector2(30, 30),
                        Colour = GetColorForButton(number),
                    },
                    new SpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Text = $"Btn {number}",
                        Font = FontUsage.Default.With(size: 16),
                        Colour = Color4.White,
                    }
                }
            };

            return container;
        }

        private static Drawable CreateComplexAnimatedContent(int number)
        {
            return new ComplexAnimatedContent(number)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(120, 120),
            };
        }

        private static Color4 GetColorForButton(int number)
        {
            Color4[] colors =
            {
                Color4.Red, Color4.Blue, Color4.Green, Color4.Yellow,
                Color4.Orange, Color4.Purple, Color4.Cyan, Color4.Pink,
                Color4.Lime, Color4.Magenta, Color4.Turquoise, Color4.Gold,
                Color4.Crimson, Color4.DarkBlue, Color4.ForestGreen, Color4.Violet
            };

            return colors[(number - 1) % colors.Length];
        }
    }

    public partial class PulsingCircle : Circle
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();
            this.Loop(c => c.ScaleTo(1.2f, 1000).Then().ScaleTo(1.0f, 1000));
        }
    }

    public partial class RotatingSquare : Box
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();
            this.Loop(c => c.RotateTo(360, 2000).Then().RotateTo(0, 0));
        }
    }

    public partial class ComplexAnimatedContent : Container
    {
        private readonly int number;

        public ComplexAnimatedContent(int number)
        {
            this.number = number;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // Create multiple animated elements
            for (int i = 0; i < 3; i++)
            {
                var circle = new PulsingCircleDelayed
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(20, 20),
                    Colour = GetColorForButton(number + i),
                    Alpha = 0.7f,
                    AnimationDelay = i * 300,
                };

                Add(circle);
            }

            // Center text
            Add(new SpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = number.ToString(),
                Font = FontUsage.Default.With(size: 20),
                Colour = Color4.White,
            });
        }

        private static Color4 GetColorForButton(int number)
        {
            Color4[] colors =
            {
                Color4.Red, Color4.Blue, Color4.Green, Color4.Yellow,
                Color4.Orange, Color4.Purple, Color4.Cyan, Color4.Pink,
                Color4.Lime, Color4.Magenta, Color4.Turquoise, Color4.Gold,
                Color4.Crimson, Color4.DarkBlue, Color4.ForestGreen, Color4.Violet
            };

            return colors[(number - 1) % colors.Length];
        }
    }

    public partial class PulsingCircleDelayed : Circle
    {
        public int AnimationDelay { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            this.Delay(AnimationDelay).Loop(c => c.ScaleTo(1.3f, 1000).Then().ScaleTo(0.8f, 1000));
        }
    }
}