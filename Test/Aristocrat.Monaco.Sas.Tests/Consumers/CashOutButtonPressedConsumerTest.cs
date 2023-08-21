namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Sas.Client;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Consumers;
    using Test.Common;

    [TestClass]
    public class CashOutButtonPressedConsumerTest
    {
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private Mock<IPropertiesManager> _propertiesManager;
        private CashOutButtonPressedConsumer _consumer;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);

            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Strict);
            _exceptionHandler.Setup(m => m.ReportException(It.IsAny<ISasExceptionCollection>())).Verifiable();

            _consumer = new CashOutButtonPressedConsumer(_exceptionHandler.Object, _propertiesManager.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConsumeExceptionSentTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { TransferOutAllowed = true });

            _consumer.Consume(new CashOutButtonPressedEvent());

            _exceptionHandler.Verify(
                m => m.ReportException(
                    It.Is<ISasExceptionCollection>(
                        ex => ex.ExceptionCode == GeneralExceptionCode.CashOutButtonPressed)));
        }

        [TestMethod]
        public void ConsumeNoExceptionSentTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { TransferOutAllowed = false });

            _consumer.Consume(new CashOutButtonPressedEvent());

            _exceptionHandler.Verify(
                m => m.ReportException(
                    It.Is<ISasExceptionCollection>(
                        ex => ex.ExceptionCode == GeneralExceptionCode.CashOutButtonPressed)),
                Times.Never());
        }
    }
}
