using osu.Framework.Allocation;
using NUnit.Framework;

namespace UntitledJBTClone.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneUntitledJBTCloneGame : UntitledJBTCloneTestScene
    {
        // Add visual tests to ensure correct behaviour of your game: https://github.com/ppy/osu-framework/wiki/Development-and-Testing
        // You can make changes to classes associated with the tests and they will recompile and update immediately.

        [BackgroundDependencyLoader]
        private void load()
        {
            AddGame(new UntitledJBTCloneGame());
        }
    }
}
