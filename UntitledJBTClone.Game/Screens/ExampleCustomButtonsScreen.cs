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
    public partial class ExampleCustomButtonsScreen : Screen
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
                    Text = "Custom Button Examples",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = FontUsage.Default.With(size: 48),
                    Colour = Color4.White,
                    Y = 100
                },
                // Custom text buttons with back button
                new ButtonGrid(CreateCustomTextButtonWithBack)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Y = -40,
                }
            };
        }

        private GameButton CreateCustomTextButtonWithBack(int number)
        {
            // Button 1 will be the back button
            if (number == 1)
            {
                var backButton = new GameButton("← Back");
                backButton.Action = () => this.Exit();
                return backButton;
            }

            string[] customTexts =
            {
                "", "Options", "Scores", "Exit", // Skip index 0 since button 1 is back
                "Level 1", "Level 2", "Level 3", "Level 4",
                "Easy", "Medium", "Hard", "Expert",
                "Red", "Blue", "Green", "Yellow"
            };

            return new GameButton(customTexts[number - 1]);
        }
    }


}