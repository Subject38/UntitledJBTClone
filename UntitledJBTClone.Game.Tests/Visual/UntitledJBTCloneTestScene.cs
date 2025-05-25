using osu.Framework.Testing;

namespace UntitledJBTClone.Game.Tests.Visual
{
    public abstract partial class UntitledJBTCloneTestScene : TestScene
    {
        protected override ITestSceneTestRunner CreateRunner() => new UntitledJBTCloneTestSceneTestRunner();

        private partial class UntitledJBTCloneTestSceneTestRunner : UntitledJBTCloneGameBase, ITestSceneTestRunner
        {
            private TestSceneTestRunner.TestRunner runner;

            protected override void LoadAsyncComplete()
            {
                base.LoadAsyncComplete();
                Add(runner = new TestSceneTestRunner.TestRunner());
            }

            public void RunTestBlocking(TestScene test) => runner.RunTestBlocking(test);
        }
    }
}
