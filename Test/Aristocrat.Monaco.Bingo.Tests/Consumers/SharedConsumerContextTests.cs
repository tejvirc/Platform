namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Bingo.Consumers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SharedConsumerContextTests
    {
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Default);
        private SharedConsumerContext _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new SharedConsumerContext(_eventBus.Object);
            _target.Initialize();
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullConstructorArgumentTest()
        {
            _target = new SharedConsumerContext(null);
        }

        [TestMethod]
        public void NameAndTypeTest()
        {
            Assert.AreEqual("Aristocrat.Monaco.Bingo.Consumers.SharedConsumerContext", _target.Name);
            Assert.IsTrue(_target.ServiceTypes.Contains(typeof(ISharedConsumer)));
            Assert.AreEqual(1, _target.ServiceTypes.Count);
        }

        [TestMethod]
        public void DisposeTest()
        {
            _eventBus.Setup(m => m.UnsubscribeAll(It.IsAny<SharedConsumerContext>())).Verifiable();

            _target.Dispose();
            _target.Dispose();  // test for dispose after already disposed

            _eventBus.Verify(m => m.UnsubscribeAll(It.IsAny<SharedConsumerContext>()), Times.Once());
        }
    }
}