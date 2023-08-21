namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Contracts.SASProperties;
    using Application.Contracts.OperatorMenu;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Consumers;
    using Test.Common;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Monaco.Sas.Progressive;

    [TestClass]
    public class OperatorMenuSettingsChangedConsumerTests
    {
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IProgressiveWinDetailsProvider> _progressiveWinDetailsProvider;
        private OperatorMenuSettingsChangedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _progressiveWinDetailsProvider = new Mock<IProgressiveWinDetailsProvider>(MockBehavior.Default);
            _progressiveWinDetailsProvider.Setup(p => p.UpdateSettings()).Verifiable();
            _target = new OperatorMenuSettingsChangedConsumer(_exceptionHandler.Object, _propertiesManager.Object, _progressiveWinDetailsProvider.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullExceptionHandlerTest()
        {
            _target = new OperatorMenuSettingsChangedConsumer(null, null, null);
        }

        [TestMethod]
        public void ConsumeTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ConfigNotification = ConfigNotificationTypes.Always });
            _exceptionHandler.Setup(
                    x => x.ReportException(
                        It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.OperatorChangedOptions)))
                .Verifiable();
            _target.Consume(new OperatorMenuSettingsChangedEvent());
            _exceptionHandler.Verify();
            _progressiveWinDetailsProvider.Verify();
        }

        [TestMethod]
        public void ConsumeExceptionNotReportedTest()
        {
            
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ConfigNotification = ConfigNotificationTypes.ExcludeSAS });
            _target.Consume(new OperatorMenuSettingsChangedEvent());
            _exceptionHandler.Verify(x => x.ReportException(
                It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.OperatorChangedOptions)), Times.Never);
            _progressiveWinDetailsProvider.Verify();
        }
    }
}
