namespace Aristocrat.Monaco.Kernel.Tests
{
    #region Using

    using System;
    using System.Reflection;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;

    #endregion

    /// <summary>
    ///     Tests for the Consumes class
    /// </summary>
    [TestClass]
    public class ConsumesTest
    {
        private EventBus _eventBus;
        private Kernel.ServiceManagerCore _serviceManager;

        // Use TestInitialize to run code before running each test
        [TestInitialize]
        public void MyTestInitialize()
        {
            _serviceManager = new Kernel.ServiceManagerCore();
            var type = typeof(ServiceManager);
            var info = type.GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);
            info?.SetValue(null, _serviceManager);

            _eventBus = new EventBus();
            _serviceManager.AddServiceAndInitialize(_eventBus);
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
            _serviceManager.RemoveService(_eventBus);
            _serviceManager.Shutdown();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorWithNullEventBusTest()
        {
            new TestConsume(null, null);
        }

        [TestMethod]
        [Ignore("Ignored, needs to be reviewed. Test are failing intermittently.")]
        public void ConstructorTest()
        {
            var target = new TestConsume(_eventBus, null);

            Assert.IsNotNull(target);
            Assert.IsFalse(target.Consumed);

            _eventBus.Publish(new TestEvent());
            Thread.Sleep(50);

            Assert.IsTrue(target.Consumed);
        }

        [TestMethod]
        public void DisposeTest()
        {
            var target = new TestConsume(_eventBus, null);

            target.Dispose();
            target.Dispose();
        }

        public class TestConsume : Consumes<TestEvent>
        {
            public TestConsume(IEventBus eventBus, object consumerContext)
                : base(eventBus, consumerContext)
            {
            }

            public bool Consumed { get; set; }

            public override void Consume(TestEvent theEvent)
            {
                Consumed = true;
            }
        }
    }
}