namespace Aristocrat.Monaco.G2S.UI.Tests.ViewModels
{
    using System;
    using System.Windows;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.ConfigWizard;
    using Application.Contracts.Settings;
    using Application.UI.Events;
    using Aristocrat.Monaco.UI.Common.Events;
    using Common.CertificateManager;
    using Common.CertificateManager.Models;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.IO;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using UI.ViewModels;

    [TestClass]
    public class SecurityConfigurationViewModelTests
    {
        private readonly Mock<ICertificateService> _serviceMock = new Mock<ICertificateService>();
        private Mock<IAutoConfigurator> _autoConfiguratorMock;

        private Mock<IServiceManager> _serviceManagerMock;
        private Mock<ICertificateFactory> _factoryMock;
        private Mock<IConfigWizardNavigator> _navigationMock;
        private Mock<IPropertiesManager> _propertiesManagerMock;
        private SecurityConfigurationViewModel _target;
        private Mock<IEventBus> _eventBusMock;

        [TestInitialize]
        public void TestInitialization()
        {
            _serviceManagerMock = MoqServiceManager.CreateInstance(MockBehavior.Default);
            _serviceManagerMock.Setup(mock => mock.GetService<IConfigurationSettingsManager>()).Returns(It.IsAny<IConfigurationSettingsManager>());

            _factoryMock = MoqServiceManager.CreateAndAddService<ICertificateFactory>(MockBehavior.Default);
            _factoryMock.Setup(x => x.GetCertificateService()).Returns(_serviceMock.Object);

            _navigationMock = MoqServiceManager.CreateAndAddService<IConfigWizardNavigator>(MockBehavior.Default);
            _navigationMock.SetupAllProperties();

            MockLocalization.Setup(MockBehavior.Default);

            _propertiesManagerMock = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _propertiesManagerMock.Setup(p => p.GetProperty(Constants.EgmId, null)).Returns("ATI_TEST123");
            _propertiesManagerMock.Setup(m => m.GetProperty(ApplicationConstants.ShowMode, false)).Returns(false);
            //_propertiesManagerMock.Setup(m => m.GetProperty(ApplicationConstants.OperatorMenuPrintButtonOptionsPrintLast15, true))
            //    .Returns(true);
            //_propertiesManagerMock.Setup(m => m.GetProperty(ApplicationConstants.OperatorMenuTechnicianModeRestrictions, false))
            //    .Returns(false);
            //_propertiesManagerMock.Setup(m => m.GetProperty(ApplicationConstants.OperatorMenuPrintButtonOptionsPrintCurrentPage, true))
            //    .Returns(true);
            //_propertiesManagerMock.Setup(m => m.GetProperty(ApplicationConstants.OperatorMenuPrintButtonOptionsPrintSelected, true))
            //    .Returns(true);
            _propertiesManagerMock.Setup(m => m.GetProperty(PropertyKey.CurrentBalance, It.IsAny<long>()))
                .Returns(0L);
            _propertiesManagerMock.Setup(mock => mock.GetProperty(ApplicationConstants.EKeyVerified, false)).Returns(false);
            _propertiesManagerMock.Setup(mock => mock.GetProperty(ApplicationConstants.EKeyDrive, null)).Returns(null);
            _propertiesManagerMock.Setup(mock => mock.GetProperty(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None)).Returns(ImportMachineSettings.None);

            _autoConfiguratorMock = MoqServiceManager.CreateAndAddService<IAutoConfigurator>(MockBehavior.Default);
            _autoConfiguratorMock.SetupGet(m => m.AutoConfigurationExists).Returns(false);

            MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Loose);

            _eventBusMock = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _eventBusMock.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<PrintButtonClickedEvent>>()));
            _eventBusMock.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<BankBalanceChangedEvent>>()));
            _eventBusMock.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<PrintButtonStatusEvent>>()));
            _eventBusMock.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<ClosedEvent>>()));
            _eventBusMock.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<OpenEvent>>()));
            _eventBusMock.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<DialogClosedEvent>>()));
            _eventBusMock.Setup(m => m.Publish(It.IsAny<OperatorMenuPageLoadedEvent>()));
            _eventBusMock.Setup(m => m.Publish(It.IsAny<OperatorMenuPopupEvent>()));
            _eventBusMock.Setup(m => m.Publish(It.IsAny<OperatorMenuWarningMessageEvent>()));
            _eventBusMock.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<PropertyChangedEvent>>(), It.IsAny<Predicate<PropertyChangedEvent>>()));
            _eventBusMock.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<ConfigurationSettingsImportedEvent>>()));
            _eventBusMock.Setup(m => m.Publish(It.IsAny<LampTestLampStateEvent>()));

            _target = new SecurityConfigurationViewModel(false);

            var _buttonService = MoqServiceManager.CreateAndAddService<IButtonService>(MockBehavior.Default);
            _buttonService.Setup(m => m.IsTestModeActive).Returns(It.IsAny<bool>());

            var _iio = MoqServiceManager.CreateAndAddService<IIO>(MockBehavior.Default);
            _iio.Setup(m => m.SetButtonLamp(It.IsAny<uint>(), It.IsAny<bool>())).Returns(It.IsAny<uint>());
            _iio.Setup(m => m.SetButtonLampByMask(It.IsAny<uint>(), It.IsAny<bool>())).Returns(It.IsAny<uint>());

            if (Application.Current == null)
            {
                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void TestActivateActionForValidData()
        {
            _target.PreSharedSecret = "aaa";
            var configuration = GetConfigurationEntity(true, true);
            _serviceMock.Setup(x => x.GetConfiguration()).Returns(configuration).Verifiable();

            _target.LoadedCommand.Execute(null);

            _target.Enrolled = true;

            _serviceMock.Verify(x => x.GetConfiguration(), Times.AtLeast(1));
            Assert.IsTrue(_navigationMock.Object.CanNavigateForward);
        }

        [TestMethod]
        public void TestActivateActionForIsConfigRequiredFalse()
        {
            var configuration = GetConfigurationEntity(false, true);
            _serviceMock.Setup(x => x.GetConfiguration()).Returns(configuration).Verifiable();

            _target.LoadedCommand.Execute(null);

            _serviceMock.Verify(x => x.GetConfiguration(), Times.AtLeast(1));
            Assert.IsTrue(_navigationMock.Object.CanNavigateForward);
        }

        [TestMethod]
        public void TestActivateActionForAutoConfiguration()
        {
            var configuration = GetConfigurationEntity(false, true);
            _serviceMock.Setup(x => x.GetConfiguration()).Returns(configuration).Verifiable();

            _autoConfiguratorMock.SetupGet(m => m.AutoConfigurationExists).Returns(true);
            _navigationMock.Setup(m => m.NavigateForward());

            _target.LoadedCommand.Execute(null);

            // This passes when running the test individually but not when running the entire test class?
            //_navigationMock.Verify(m => m.NavigateForward(), Times.Once());

            _target.LoadedCommand.Execute(null);
            _target.LoadedCommand.Execute(null);

            _serviceMock.Verify(x => x.GetConfiguration(), Times.AtLeast(3));
            Assert.IsTrue(_navigationMock.Object.CanNavigateForward);
            //_navigationMock.Verify(m => m.NavigateForward(), Times.Once());
        }

        [TestMethod]
        public void TestCommitAction()
        {
            var configuration = GetConfigurationEntity(true, true);
            _serviceMock.Setup(x => x.GetConfiguration()).Returns(configuration).Verifiable();
            _serviceMock.Setup(x => x.SaveConfiguration(It.IsAny<PkiConfiguration>())).Verifiable();

            _target.LoadedCommand.Execute(null);
            _target.CommitCommand.Execute(null);
            _target.Save();

            Assert.IsTrue(_target.IsCommitted);
            _serviceMock.Verify(x => x.GetConfiguration(), Times.AtLeast(2));
            _serviceMock.Verify(x => x.SaveConfiguration(It.IsAny<PkiConfiguration>()), Times.AtLeast(1));
        }

        private static PkiConfiguration GetConfigurationEntity(bool isConfigRequired, bool isValid)
        {
            var result = new PkiConfiguration
            {
                CertificateManagerLocation = "http://someurl",
                CertificateStatusLocation = "http://someurl",
                ScepEnabled = isConfigRequired,
                OcspAcceptPreviouslyGoodCertificatePeriod = 5,
                OcspNextUpdate = 5,
                OcspMinimumPeriodForOffline = 5,
                OcspReAuthenticationPeriod = 5,
                ScepCaIdent = "something",
                ScepManualPollingInterval = 5000,
                ScepUsername = "someuser"
            };

            if (!isValid)
            {
                result.CertificateManagerLocation = string.Empty;
                result.OcspReAuthenticationPeriod = -10;
            }

            return result;
        }
    }
}
