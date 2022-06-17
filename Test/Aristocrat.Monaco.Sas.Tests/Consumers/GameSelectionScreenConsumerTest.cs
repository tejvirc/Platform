namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using Aristocrat.Sas.Client;
    using Gaming.Contracts;
    using Kernel;
    using Sas.Consumers;
    using Sas.Exceptions;
    using Test.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GameSelectionScreenConsumerTest
    {
        private Mock<ISasExceptionHandler> _exceptionHandlerMock;

        private GameSelectionScreenConsumer _target;

        [TestInitialize]
        public void Initialize()
        {
            _exceptionHandlerMock = new Mock<ISasExceptionHandler>(MockBehavior.Strict);

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _target = new GameSelectionScreenConsumer(_exceptionHandlerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataTestMethod]
        [DataRow(true, DisplayName = "Entering, $8C sent")]
        [DataRow(false, DisplayName = "Exiting, $8C not sent")]
        public void ConsumeEventTestUnexpectedExit(bool isEntering)
        {
            GameSelectedExceptionBuilder actual = null;

            _exceptionHandlerMock.Setup(m => m.ReportException(It.IsAny<GameSelectedExceptionBuilder>()))
                .Callback((ISasExceptionCollection g) => actual = g as GameSelectedExceptionBuilder)
                .Verifiable();

            var gameSelectedEvent = new GameSelectionScreenEvent(isEntering);

            _target.Consume(gameSelectedEvent);

            if (isEntering)
            {
                Assert.IsNotNull(actual);
                _exceptionHandlerMock.Verify(m => m.ReportException(It.IsAny<GameSelectedExceptionBuilder>()), Times.Once);
            }
            else
            {
                Assert.IsNull(actual);
                _exceptionHandlerMock.Verify(m => m.ReportException(It.IsAny<GameSelectedExceptionBuilder>()), Times.Never);
            }
        }
    }
}