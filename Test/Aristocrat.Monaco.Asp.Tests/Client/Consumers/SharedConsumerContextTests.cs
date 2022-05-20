namespace Aristocrat.Monaco.Asp.Tests.Client.Consumers
{
    using Aristocrat.Monaco.Asp.Client.Consumers;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;

    [TestClass]
    public class SharedConsumerContextTests
    {
        private readonly string Name = $"Aristocrat.Monaco.Asp.Client.Consumers.SharedConsumerContext";
        private SharedConsumerContext _target;
        private Mock<IEventBus> _eventBus;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _target = new SharedConsumerContext();
            _target.Initialize();
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual(Name, _target.Name);
        }

        [TestMethod]
        public void DisposeTest()
        {
            _eventBus.Setup(mock => mock.UnsubscribeAll(_target)).Verifiable();

            _target.Dispose();

            _eventBus.VerifyAll();

            _target.Dispose();
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            ICollection<Type> serviceTypes = _target.ServiceTypes;
            Assert.AreEqual(1, serviceTypes.Count);
            Assert.IsTrue(serviceTypes.Contains(typeof(ISharedConsumer)));
        }
    }
}
