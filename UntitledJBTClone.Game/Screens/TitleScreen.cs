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
    public partial class TitleScreen : Screen
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
                    Text = "Untitled JBT Clone",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = FontUsage.Default.With(size: 48),
                    Colour = Color4.White,
                    Y = 100
                },
                // Button grid with transition button
                new ButtonGrid(CreateTitleScreenButton)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Y = -40, // 40px margin from bottom
                }
            };
        }

        private GameButton CreateTitleScreenButton(int number)
        {
            // Button 15 will be the profile select button
            if (number == 15)
            {
                var profileButton = new GameButton("Profile");
                profileButton.Action = () => this.Push(new ProfileSelectScreen());
                return profileButton;
            }

            // Button 16 will be the transition button to Advanced Screen
            if (number == 16)
            {
                var transitionButton = new GameButton("Demo");
                transitionButton.Action = () => this.Push(new AdvancedButtonExampleScreen());
                return transitionButton;
            }

            // All other buttons are numbered normally
            return new GameButton(number);
        }
    }


}