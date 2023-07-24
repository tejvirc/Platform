namespace Aristocrat.Monaco.G2S.Tests
{
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Emdi;
    using Common.CertificateManager;
    using G2S.Meters;
    using G2S.Services;
    using Hardware.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using Application.Contracts.Protocol;
    using Aristocrat.Cabinet;
    using Aristocrat.Monaco.Application.Protocol;
    using Aristocrat.Monaco.G2S.Security;
    using Aristocrat.Monaco.Hardware.Services;

    [TestClass]
    public class G2SEngineTest
    {
        private readonly Mock<IG2SEgm> _egm  = new Mock<IG2SEgm>();
        private readonly Mock<IPropertiesManager> _properties  = new Mock<IPropertiesManager>();
        private readonly Mock<IHostFactory> _hostFactory  = new Mock<IHostFactory>();
        private readonly Mock<IDeviceFactory> _deviceFactory  = new Mock<IDeviceFactory>();
        private readonly Mock<IScriptManager> _scriptManager  = new Mock<IScriptManager>();
        private readonly Mock<IPackageDownloadManager> _packageDownloadManager  = new Mock<IPackageDownloadManager>();
        private readonly Mock<IDeviceObserver> _deviceObserver  = new Mock<IDeviceObserver>();
        private readonly Mock<IProgressiveDeviceManager> _progressiveDeviceManager  = new Mock<IProgressiveDeviceManager>();
        private readonly Mock<IEgmStateObserver> _egmStateObserver  = new Mock<IEgmStateObserver>();
        private readonly Mock<IDeviceRegistryService> _deviceRegistryService  = new Mock<IDeviceRegistryService>();
        private readonly Mock<IGatComponentFactory> _gatComponentFactory  = new Mock<IGatComponentFactory>();
        private readonly Mock<IMetersSubscriptionManager> _meterSubManager  = new Mock<IMetersSubscriptionManager>();
        private readonly Mock<IG2SMeterProvider> _g2sMeterProvider  = new Mock<IG2SMeterProvider>();
        private readonly Mock<IVoucherDataService> _voucherDataService  = new Mock<IVoucherDataService>();
        private readonly Mock<IMasterResetService> _masterResetService  = new Mock<IMasterResetService>();
        private readonly Mock<ISelfTest> _clientStatus  = new Mock<ISelfTest>();
        private readonly Mock<ICertificateService> _certificates  = new Mock<ICertificateService>();
        private readonly Mock<ICertificateMonitor> _certificateMonitor  = new Mock<ICertificateMonitor>();
        private readonly Mock<IEmdi> _emdi  = new Mock<IEmdi>();
        private readonly Mock<ICentralService> _central  = new Mock<ICentralService>();
        private readonly Mock<IEventLift> _eventLift  = new Mock<IEventLift>();
        private readonly Mock<IMultiProtocolConfigurationProvider> _multiProtocolConfigurationProvider  = new Mock<IMultiProtocolConfigurationProvider>();

        [DataRow(true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, DisplayName = "Null Egm test")]
        [DataRow(false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, DisplayName = "Null PropertiesManager test")]
        [DataRow(false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, DisplayName = "Null HostFactory test")]
        [DataRow(false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, DisplayName = "Null DeviceFactory test")]
        [DataRow(false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, DisplayName = "Null ScriptManager test")]
        [DataRow(false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, DisplayName = "Null PackageDownloadManager test")]
        [DataRow(false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, DisplayName = "Null DeviceObserver test")]
        [DataRow(false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, DisplayName = "Null ProgressiveDeviceManager test")]
        [DataRow(false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, DisplayName = "Null EgmStateObserver test")]
        [DataRow(false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, DisplayName = "Null DeviceRegistryService test")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, DisplayName = "Null GatComponentFactory test")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, DisplayName = "Null MetersSubscriptionManager test")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, DisplayName = "Null G2SMeterProvider test")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, DisplayName = "Null VoucherDataService test")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, DisplayName = "Null MasterResetService test")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, DisplayName = "Null SelfTest test")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, DisplayName = "Null CertificateService test")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, DisplayName = "Null CertificateMonitor test")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, DisplayName = "Null Emdi test")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, DisplayName = "Null CentralService test")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, DisplayName = "Null EventLift test")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, DisplayName = "Null MultiProtocolConfigurationProvider test")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithInvalidParamsExpectException(
            bool nullEgm,
            bool nullProperties,
            bool nullHostFactory,
            bool nullDeviceFactory,
            bool nullScriptManager,
            bool nullPackageDownloadManager,
            bool nullDeviceObserver,
            bool nullProgressiveDeviceManager,
            bool nullEgmStateObserver,
            bool nullDeviceRegistryService,
            bool nullGatComponentFactory,
            bool nullMeterSubManager,
            bool nullG2SMeterProvider,
            bool nullVoucherDataService,
            bool nullMasterResetService,
            bool nullClientStatus,
            bool nullCertificates,
            bool nullCertificateMonitor,
            bool nullEmdi,
            bool nullCentral,
            bool nullEventLift,
            bool nullMultiProtocolConfigurationProvider)
        {
            _ = CreateG2SEngine(
                nullEgm,
                nullProperties,
                nullHostFactory,
                nullDeviceFactory,
                nullScriptManager,
                nullPackageDownloadManager,
                nullDeviceObserver,
                nullProgressiveDeviceManager,
                nullEgmStateObserver,
                nullDeviceRegistryService,
                nullGatComponentFactory,
                nullMeterSubManager,
                nullG2SMeterProvider,
                nullVoucherDataService,
                nullMasterResetService,
                nullClientStatus,
                nullCertificates,
                nullCertificateMonitor,
                nullEmdi,
                nullCentral,
                nullEventLift,
                nullMultiProtocolConfigurationProvider
            );
        }

        [TestMethod]
        public void WhenStopExpectEgmStopped()
        {
            var service = CreateG2SEngine();

            service.Stop();

            _egm.Verify(e => e.Stop());
        }

        private G2SEngine CreateG2SEngine(
            bool nullEgm = false,
            bool nullProperties = false,
            bool nullHostFactory = false,
            bool nullDeviceFactory = false,
            bool nullScriptManager = false,
            bool nullPackageDownloadManager = false,
            bool nullDeviceObserver = false,
            bool nullProgressiveDeviceManager = false,
            bool nullEgmStateObserver = false,
            bool nullDeviceRegistryService = false,
            bool nullGatComponentFactory = false,
            bool nullMeterSubManager = false,
            bool nullG2SMeterProvider = false,
            bool nullVoucherDataService = false,
            bool nullMasterResetService = false,
            bool nullClientStatus = false,
            bool nullCertificates = false,
            bool nullCertificateMonitor = false,
            bool nullEmdi = false,
            bool nullCentral = false,
            bool nullEventLift = false,
            bool nullMultiProtocolConfigurationProvider = false
        )
        {
            return new G2SEngine(
                nullEgm ? null : _egm.Object,
                nullProperties ? null : _properties.Object,
                nullHostFactory ? null : _hostFactory.Object,
                nullDeviceFactory ? null : _deviceFactory.Object,
                nullScriptManager ? null : _scriptManager.Object,
                nullPackageDownloadManager ? null : _packageDownloadManager.Object,
                nullDeviceObserver ? null : _deviceObserver.Object,
                nullProgressiveDeviceManager ? null : _progressiveDeviceManager.Object,
                nullEgmStateObserver ? null : _egmStateObserver.Object,
                nullDeviceRegistryService ? null : _deviceRegistryService.Object,
                nullGatComponentFactory ? null : _gatComponentFactory.Object,
                nullMeterSubManager ? null : _meterSubManager.Object,
                nullG2SMeterProvider ? null : _g2sMeterProvider.Object,
                nullVoucherDataService ? null : _voucherDataService.Object,
                nullMasterResetService ? null : _masterResetService.Object,
                nullClientStatus ? null : _clientStatus.Object,
                nullCertificates ? null : _certificates.Object,
                nullCertificateMonitor ? null : _certificateMonitor.Object,
                nullEmdi ? null : _emdi.Object,
                nullCentral ? null : _central.Object,
                nullEventLift ? null : _eventLift.Object,
                nullMultiProtocolConfigurationProvider ? null : _multiProtocolConfigurationProvider.Object);
        }
    }
}
