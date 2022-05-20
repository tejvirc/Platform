namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Consumers;
    using Test.Common;

    [TestClass]
    public class PlayerRequestedDrawConsumerTests
    {
        private PlayerRequestedDrawConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Strict);
        private readonly Mock<IRteStatusProvider> _rteProvider = new Mock<IRteStatusProvider>(MockBehavior.Strict);
        private readonly Mock<ISasExceptionHandler> _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Strict);
        private const byte SasClient1 = 0;
        private const byte SasClient2 = 1;

        [TestInitialize]
        public void TestInitialization()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _eventBus.Setup(m => m.Subscribe(
                It.IsAny<object>(),
                It.IsAny<Action<CardsHeldEvent>>(),
                It.IsAny<Predicate<CardsHeldEvent>>())).Verifiable();
            _target = new PlayerRequestedDrawConsumer(_rteProvider.Object, _exceptionHandler.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullRteProviderTest()
        {
            _target = new PlayerRequestedDrawConsumer(null, _exceptionHandler.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullExceptionHandlerTest()
        {
            _target = new PlayerRequestedDrawConsumer(_rteProvider.Object, null);
        }

        [TestMethod]
        public void ConsumeRteOnBothTest()
        {
            _rteProvider.Setup(m => m.Client1RteEnabled).Returns(true);
            _rteProvider.Setup(m => m.Client2RteEnabled).Returns(true);
            _exceptionHandler.Setup(m => m.ReportException(It.IsAny<ISasExceptionCollection>(), SasClient1)).Verifiable();
            _exceptionHandler.Setup(m => m.ReportException(It.IsAny<ISasExceptionCollection>(), SasClient2)).Verifiable();

            _target.Consume(new PlayerRequestedDrawEvent());

            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<ISasExceptionCollection>(), SasClient1), Times.Once);
            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<ISasExceptionCollection>(), SasClient2), Times.Once);
        }

        [TestMethod]
        public void ConsumeRteOnClient1Test()
        {
            _rteProvider.Setup(m => m.Client1RteEnabled).Returns(true);
            _rteProvider.Setup(m => m.Client2RteEnabled).Returns(false);
            _exceptionHandler.Setup(m => m.ReportException(It.IsAny<ISasExceptionCollection>(), SasClient1)).Verifiable();
            _exceptionHandler.Setup(m => m.ReportException(It.IsAny<ISasExceptionCollection>(), SasClient2)).Verifiable();

            _target.Consume(new PlayerRequestedDrawEvent());

            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<ISasExceptionCollection>(), SasClient1), Times.Once);
            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<ISasExceptionCollection>(), SasClient2), Times.Never);
        }

        [TestMethod]
        public void ConsumeRteOnClient2Test()
        {
            _rteProvider.Setup(m => m.Client1RteEnabled).Returns(false);
            _rteProvider.Setup(m => m.Client2RteEnabled).Returns(true);
            _exceptionHandler.Setup(m => m.ReportException(It.IsAny<ISasExceptionCollection>(), SasClient1)).Verifiable();
            _exceptionHandler.Setup(m => m.ReportException(It.IsAny<ISasExceptionCollection>(), SasClient2)).Verifiable();

            _target.Consume(new PlayerRequestedDrawEvent());

            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<ISasExceptionCollection>(), SasClient1), Times.Never);
            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<ISasExceptionCollection>(), SasClient2), Times.Once);
        }
    }
}
