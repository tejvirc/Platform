namespace Aristocrat.Monaco.Kernel.Tests
{
    using Contracts;
    using Contracts.Events;
    using Kernel.Debugging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class InitializationProviderTests
    {
        private Mock<IEventBus> _eventBus;
        private InitializationProvider _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            MoqServiceManager.CreateAndAddService<IDebuggerService>(MockBehavior.Strict)
                .Setup(service => service.AttachDebuggerIfRequestedForPoint(It.IsAny<DebuggerAttachPoint>()))
                .Returns(false);
			_target = new InitializationProvider();
            _target.Initialize();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual("InitializationProvider", _target.Name);
        }

        [TestMethod]
        public void ServiceTypeTest()
        {
            var types = _target.ServiceTypes;
            Assert.IsTrue(types.Contains(typeof(IInitializationProvider)));
            Assert.AreEqual(1, types.Count);
        }

        [TestMethod]
        public void ComponentInitializationCompletedTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<InitializationCompletedEvent>())).Verifiable();
            Assert.IsFalse(_target.IsSystemInitializationComplete);

            _target.SystemInitializationCompleted();

            Assert.IsTrue(_target.IsSystemInitializationComplete);
            _eventBus.Verify();
        }
    }
}
