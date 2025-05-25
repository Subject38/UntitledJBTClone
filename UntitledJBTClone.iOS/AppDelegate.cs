using osu.Framework.iOS;
using UntitledJBTClone.Game;

namespace UntitledJBTClone.iOS
{
    /// <inheritdoc />
    public class AppDelegate : GameApplicationDelegate
    {
        /// <inheritdoc />
        protected override osu.Framework.Game CreateGame() => new UntitledJBTCloneGame();
    }
}
