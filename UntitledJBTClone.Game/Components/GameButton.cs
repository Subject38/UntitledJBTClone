using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Graphics;

namespace UntitledJBTClone.Game.Components
{
    public partial class GameButton : Button
    {
        private Box background;
        private Container contentContainer;
        private readonly Drawable customContent;

        public GameButton(Drawable content = null)
        {
            customContent = content;
            Size = new Vector2(160, 160);
        }

        // Convenience constructor for text-only buttons
        public GameButton(string text) : this(CreateTextContent(text))
        {
        }

        // Convenience constructor for numbered buttons (backward compatibility)
        public GameButton(int number) : this(number.ToString())
        {
        }

        private static Drawable CreateTextContent(string text)
        {
            return new SpriteText
            {
                Text = text,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = FontUsage.Default.With(size: 24),
                Colour = Color4.White,
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.DarkGray,
                },
                contentContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = customContent ?? CreateTextContent(""),
                }
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeColour(Color4.Gray, 200);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeColour(Color4.DarkGray, 200);
            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            background.FlashColour(Color4.White, 100);
            // Add button click logic here
            return base.OnClick(e);
        }
    }
}