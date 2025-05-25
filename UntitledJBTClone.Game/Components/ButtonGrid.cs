using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace UntitledJBTClone.Game.Components
{
    public partial class ButtonGrid : Container
    {
        private readonly Func<int, GameButton> buttonFactory;

        public ButtonGrid(Func<int, GameButton> customButtonFactory = null)
        {
            buttonFactory = customButtonFactory ?? DefaultButtonFactory;

            // 4 buttons * 160 + 3 gaps * 37 + 2 edge margins * 8 = 640 + 111 + 16 = 767
            // 4 buttons * 160 + 3 gaps * 37 + 2 edge margins * 6 = 640 + 111 + 12 = 763
            Size = new Vector2(767, 763);

            Child = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                RowDimensions =
                [
                    new Dimension(GridSizeMode.Absolute, 6), // top edge margin
                    new Dimension(GridSizeMode.Absolute, 160),
                    new Dimension(GridSizeMode.Absolute, 37), // gap
                    new Dimension(GridSizeMode.Absolute, 160),
                    new Dimension(GridSizeMode.Absolute, 37), // gap
                    new Dimension(GridSizeMode.Absolute, 160),
                    new Dimension(GridSizeMode.Absolute, 37), // gap
                    new Dimension(GridSizeMode.Absolute, 160),
                    new Dimension(GridSizeMode.Absolute, 6), // bottom edge margin
                ],
                ColumnDimensions =
                [
                    new Dimension(GridSizeMode.Absolute, 8), // left edge margin
                    new Dimension(GridSizeMode.Absolute, 160),
                    new Dimension(GridSizeMode.Absolute, 37), // gap
                    new Dimension(GridSizeMode.Absolute, 160),
                    new Dimension(GridSizeMode.Absolute, 37), // gap
                    new Dimension(GridSizeMode.Absolute, 160),
                    new Dimension(GridSizeMode.Absolute, 37), // gap
                    new Dimension(GridSizeMode.Absolute, 160),
                    new Dimension(GridSizeMode.Absolute, 8), // right edge margin
                ],
                Content = new[]
                {
                    // Top edge margin row
                    new Drawable[] { null, null, null, null, null, null, null, null, null },
                    // Row 1
                    [null, CreateButton(1), null, CreateButton(2), null, CreateButton(3), null, CreateButton(4), null],
                    // Gap row
                    [null, null, null, null, null, null, null, null, null],
                    // Row 2
                    [null, CreateButton(5), null, CreateButton(6), null, CreateButton(7), null, CreateButton(8), null],
                    // Gap row
                    [null, null, null, null, null, null, null, null, null],
                    // Row 3
                    [null, CreateButton(9), null, CreateButton(10), null, CreateButton(11), null, CreateButton(12), null],
                    // Gap row
                    [null, null, null, null, null, null, null, null, null],
                    // Row 4
                    [null, CreateButton(13), null, CreateButton(14), null, CreateButton(15), null, CreateButton(16), null],
                    // Bottom edge margin row
                    [null, null, null, null, null, null, null, null, null],
                }
            };
        }

        private static GameButton DefaultButtonFactory(int number)
        {
            return new GameButton(number);
        }

        private Drawable CreateButton(int number)
        {
            return buttonFactory(number);
        }
    }
}