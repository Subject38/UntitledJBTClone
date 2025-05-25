using osu.Framework.Platform;
using osu.Framework;
using UntitledJBTClone.Game;

namespace UntitledJBTClone.Desktop
{
    public static class Program
    {
        public static void Main()
        {
            using (GameHost host = Host.GetSuitableDesktopHost(@"UntitledJBTClone"))
            using (osu.Framework.Game game = new UntitledJBTCloneGame())
                host.Run(game);
        }
    }
}
