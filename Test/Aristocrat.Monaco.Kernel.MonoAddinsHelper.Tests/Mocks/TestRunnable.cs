namespace Aristocrat.Monaco.Kernel.Tests.Mocks
{
    using System.Threading;

    public class TestRunnable : BaseRunnable
    {
        public bool ThreadIdMatches { get; set; }

        public TestEvent TestEvent { get; set; }

        public void DoTestInvoke()
        {
            InvokeReceiver();
        }

        protected override void OnInitialize()
        {
            ServiceManager.GetInstance().GetService<IEventBus>().Subscribe<TestEvent>(this, TestEventReceiver);
        }

        protected override void OnRun()
        {
        }

        protected override void OnStop()
        {
        }

        private void TestEventReceiver(TestEvent te)
        {
            TestEvent = te;
        }

        private void InvokeReceiver()
        {
        }
    }
}