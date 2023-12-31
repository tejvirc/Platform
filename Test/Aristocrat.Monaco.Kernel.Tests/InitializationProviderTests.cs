﻿namespace Aristocrat.Monaco.Kernel.Tests
{
    using Components;
    using Contracts;
    using Contracts.Events;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class InitializationProviderTests
    {
        private readonly InitializationProvider _target = new InitializationProvider();
        private Mock<IEventBus> _eventBus;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
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
