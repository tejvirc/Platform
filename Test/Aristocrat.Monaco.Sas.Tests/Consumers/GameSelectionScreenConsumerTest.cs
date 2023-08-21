namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using Aristocrat.Sas.Client;
    using Gaming.Contracts;
    using Kernel;
    using Sas.Consumers;
    using Sas.Contracts.SASProperties;
    using Sas.Exceptions;
    using Test.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GameSelectionScreenConsumerTest
    {
        private Mock<ISasExceptionHandler> _exceptionHandlerMock;
        private Mock<IPropertiesManager> _propertiesManager;

        private GameSelectionScreenConsumer _target;

        [TestInitialize]
        public void Initialize()
        {
            _exceptionHandlerMock = new Mock<ISasExceptionHandler>(MockBehavior.Strict);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _target = new GameSelectionScreenConsumer(
                _exceptionHandlerMock.Object,
                _propertiesManager.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataTestMethod]
        [DataRow(false, false, DisplayName = "Exiting, $8C not sent")]
        [DataRow(true, false, DisplayName = "Entering, different ID, $8C sent")]
        [DataRow(true, true, DisplayName = "Entering, same ID, $8C not sent")]
        public void ConsumeEventTestUnexpectedExit(bool isEntering, bool sameId)
        {
            GameSelectedExceptionBuilder actual = null;

            _exceptionHandlerMock.Setup(m => m.ReportException(It.IsAny<GameSelectedExceptionBuilder>()))
                .Callback((ISasExceptionCollection g) => actual = g as GameSelectedExceptionBuilder)
                .Verifiable();

            _propertiesManager.Setup(p => p.SetProperty(SasProperties.PreviousSelectedGameId, It.IsAny<int>()));
            _propertiesManager.Setup(p => p.GetProperty(SasProperties.PreviousSelectedGameId, It.IsAny<int>()))
                .Returns(sameId ? 0 : 1);

            var gameSelectedEvent = new GameSelectionScreenEvent(isEntering);

            _target.Consume(gameSelectedEvent);

            if (isEntering && !sameId)
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