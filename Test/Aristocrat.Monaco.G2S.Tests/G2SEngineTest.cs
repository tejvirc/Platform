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

    [TestClass]
    public class G2SEngineTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var service = new G2SEngine(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPropertyManagerExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var service = new G2SEngine(
                egm.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullHostFactoryExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();

            var service = new G2SEngine(
                egm.Object,
                properties.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullScriptManagerExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();
            var hostFactory = new Mock<IHostFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var service = new G2SEngine(
                egm.Object,
                properties.Object,
                hostFactory.Object,
                deviceFactory.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullDeviceObserverExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();
            var hostFactory = new Mock<IHostFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var scriptManager = new Mock<IScriptManager>();
            var service = new G2SEngine(
                egm.Object,
                properties.Object,
                hostFactory.Object,
                deviceFactory.Object,
                scriptManager.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmStateObserverExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();
            var hostFactory = new Mock<IHostFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var scriptManager = new Mock<IScriptManager>();
            var packageDownloadManager = new Mock<IPackageDownloadManager>();
            var deviceObserver = new Mock<IDeviceObserver>();
            var service = new G2SEngine(
                egm.Object,
                properties.Object,
                hostFactory.Object,
                deviceFactory.Object,
                scriptManager.Object,
                packageDownloadManager.Object,
                deviceObserver.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullProgressiveStateObserverExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();
            var hostFactory = new Mock<IHostFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var scriptManager = new Mock<IScriptManager>();
            var packageDownloadManager = new Mock<IPackageDownloadManager>();
            var deviceObserver = new Mock<IDeviceObserver>();
            var progressiveDeviceObserver = new Mock<IProgressiveDeviceObserver>();
            var service = new G2SEngine(
                egm.Object,
                properties.Object,
                hostFactory.Object,
                deviceFactory.Object,
                scriptManager.Object,
                packageDownloadManager.Object,
                deviceObserver.Object,
                progressiveDeviceObserver.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullDeviceRegistryServiceExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();
            var hostFactory = new Mock<IHostFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var scriptManager = new Mock<IScriptManager>();
            var packageDownloadManager = new Mock<IPackageDownloadManager>();
            var deviceObserver = new Mock<IDeviceObserver>();
            var progressiveDeviceObserver = new Mock<IProgressiveDeviceObserver>();
            var egmStateObserver = new Mock<IEgmStateObserver>();
            var service = new G2SEngine(
                egm.Object,
                properties.Object,
                hostFactory.Object,
                deviceFactory.Object,
                scriptManager.Object,
                packageDownloadManager.Object,
                deviceObserver.Object,
                progressiveDeviceObserver.Object,
                egmStateObserver.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullGatComponentFactoryExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();
            var hostFactory = new Mock<IHostFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var scriptManager = new Mock<IScriptManager>();
            var packageDownloadManager = new Mock<IPackageDownloadManager>();
            var deviceObserver = new Mock<IDeviceObserver>();
            var progressiveDeviceObserver = new Mock<IProgressiveDeviceObserver>();
            var egmStateObserver = new Mock<IEgmStateObserver>();
            var deviceRegistryService = new Mock<IDeviceRegistryService>();
            var service = new G2SEngine(
                egm.Object,
                properties.Object,
                hostFactory.Object,
                deviceFactory.Object,
                scriptManager.Object,
                packageDownloadManager.Object,
                deviceObserver.Object,
                progressiveDeviceObserver.Object,
                egmStateObserver.Object,
                deviceRegistryService.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullMetersSubscriptionManagerExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();
            var hostFactory = new Mock<IHostFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var scriptManager = new Mock<IScriptManager>();
            var packageDownloadManager = new Mock<IPackageDownloadManager>();
            var deviceObserver = new Mock<IDeviceObserver>();
            var progressiveDeviceObserver = new Mock<IProgressiveDeviceObserver>();
            var egmStateObserver = new Mock<IEgmStateObserver>();
            var deviceRegistryService = new Mock<IDeviceRegistryService>();
            var gatComponentFactory = new Mock<IGatComponentFactory>();
            var service = new G2SEngine(
                egm.Object,
                properties.Object,
                hostFactory.Object,
                deviceFactory.Object,
                scriptManager.Object,
                packageDownloadManager.Object,
                deviceObserver.Object,
                progressiveDeviceObserver.Object,
                egmStateObserver.Object,
                deviceRegistryService.Object,
                gatComponentFactory.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullG2SMeterProviderExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();
            var hostFactory = new Mock<IHostFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var scriptManager = new Mock<IScriptManager>();
            var packageDownloadManager = new Mock<IPackageDownloadManager>();
            var deviceObserver = new Mock<IDeviceObserver>();
            var progressiveDeviceObserver = new Mock<IProgressiveDeviceObserver>();
            var egmStateObserver = new Mock<IEgmStateObserver>();
            var deviceRegistryService = new Mock<IDeviceRegistryService>();
            var gatComponentFactory = new Mock<IGatComponentFactory>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var service = new G2SEngine(
                egm.Object,
                properties.Object,
                hostFactory.Object,
                deviceFactory.Object,
                scriptManager.Object,
                packageDownloadManager.Object,
                deviceObserver.Object,
                progressiveDeviceObserver.Object,
                egmStateObserver.Object,
                deviceRegistryService.Object,
                gatComponentFactory.Object,
                meterSubManager.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullVoucherDataServiceExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();
            var hostFactory = new Mock<IHostFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var scriptManager = new Mock<IScriptManager>();
            var packageDownloadManager = new Mock<IPackageDownloadManager>();
            var deviceObserver = new Mock<IDeviceObserver>();
            var progressiveDeviceObserver = new Mock<IProgressiveDeviceObserver>();
            var egmStateObserver = new Mock<IEgmStateObserver>();
            var deviceRegistryService = new Mock<IDeviceRegistryService>();
            var gatComponentFactory = new Mock<IGatComponentFactory>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var g2sMeterProvider = new Mock<IG2SMeterProvider>();
            var service = new G2SEngine(
                egm.Object,
                properties.Object,
                hostFactory.Object,
                deviceFactory.Object,
                scriptManager.Object,
                packageDownloadManager.Object,
                deviceObserver.Object,
                progressiveDeviceObserver.Object,
                egmStateObserver.Object,
                deviceRegistryService.Object,
                gatComponentFactory.Object,
                meterSubManager.Object,
                g2sMeterProvider.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullMasterResetExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();
            var hostFactory = new Mock<IHostFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var scriptManager = new Mock<IScriptManager>();
            var packageDownloadManager = new Mock<IPackageDownloadManager>();
            var deviceObserver = new Mock<IDeviceObserver>();
            var progressiveDeviceObserver = new Mock<IProgressiveDeviceObserver>();
            var egmStateObserver = new Mock<IEgmStateObserver>();
            var deviceRegistryService = new Mock<IDeviceRegistryService>();
            var gatComponentFactory = new Mock<IGatComponentFactory>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var g2sMeterProvider = new Mock<IG2SMeterProvider>();
            var voucherDataService = new Mock<IVoucherDataService>();
            var service = new G2SEngine(
                egm.Object,
                properties.Object,
                hostFactory.Object,
                deviceFactory.Object,
                scriptManager.Object,
                packageDownloadManager.Object,
                deviceObserver.Object,
                progressiveDeviceObserver.Object,
                egmStateObserver.Object,
                deviceRegistryService.Object,
                gatComponentFactory.Object,
                meterSubManager.Object,
                g2sMeterProvider.Object,
                voucherDataService.Object,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullSelfTestExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();
            var hostFactory = new Mock<IHostFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var scriptManager = new Mock<IScriptManager>();
            var packageDownloadManager = new Mock<IPackageDownloadManager>();
            var deviceObserver = new Mock<IDeviceObserver>();
            var progressiveDeviceObserver = new Mock<IProgressiveDeviceObserver>();
            var egmStateObserver = new Mock<IEgmStateObserver>();
            var deviceRegistryService = new Mock<IDeviceRegistryService>();
            var gatComponentFactory = new Mock<IGatComponentFactory>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var g2sMeterProvider = new Mock<IG2SMeterProvider>();
            var voucherDataService = new Mock<IVoucherDataService>();
            var masterResetService = new Mock<IMasterResetService>();

            var service = new G2SEngine(
                egm.Object,
                properties.Object,
                hostFactory.Object,
                deviceFactory.Object,
                scriptManager.Object,
                packageDownloadManager.Object,
                deviceObserver.Object,
                progressiveDeviceObserver.Object,
                egmStateObserver.Object,
                deviceRegistryService.Object,
                gatComponentFactory.Object,
                meterSubManager.Object,
                g2sMeterProvider.Object,
                voucherDataService.Object,
                masterResetService.Object,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullICertificateServiceExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();
            var hostFactory = new Mock<IHostFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var scriptManager = new Mock<IScriptManager>();
            var packageDownloadManager = new Mock<IPackageDownloadManager>();
            var deviceObserver = new Mock<IDeviceObserver>();
            var progressiveDeviceObserver = new Mock<IProgressiveDeviceObserver>();
            var egmStateObserver = new Mock<IEgmStateObserver>();
            var deviceRegistryService = new Mock<IDeviceRegistryService>();
            var gatComponentFactory = new Mock<IGatComponentFactory>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var g2sMeterProvider = new Mock<IG2SMeterProvider>();
            var voucherDataService = new Mock<IVoucherDataService>();
            var masterResetService = new Mock<IMasterResetService>();
            var clientStatus = new Mock<ISelfTest>();

            var service = new G2SEngine(
                egm.Object,
                properties.Object,
                hostFactory.Object,
                deviceFactory.Object,
                scriptManager.Object,
                packageDownloadManager.Object,
                deviceObserver.Object,
                progressiveDeviceObserver.Object,
                egmStateObserver.Object,
                deviceRegistryService.Object,
                gatComponentFactory.Object,
                meterSubManager.Object,
                g2sMeterProvider.Object,
                voucherDataService.Object,
                masterResetService.Object,
                clientStatus.Object,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCertificateMonitorExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();
            var hostFactory = new Mock<IHostFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var scriptManager = new Mock<IScriptManager>();
            var packageDownloadManager = new Mock<IPackageDownloadManager>();
            var deviceObserver = new Mock<IDeviceObserver>();
            var progressiveDeviceObserver = new Mock<IProgressiveDeviceObserver>();
            var egmStateObserver = new Mock<IEgmStateObserver>();
            var deviceRegistryService = new Mock<IDeviceRegistryService>();
            var gatComponentFactory = new Mock<IGatComponentFactory>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var g2sMeterProvider = new Mock<IG2SMeterProvider>();
            var voucherDataService = new Mock<IVoucherDataService>();
            var masterResetService = new Mock<IMasterResetService>();
            var clientStatus = new Mock<ISelfTest>();
            var certificates = new Mock<ICertificateService>();

            var service = new G2SEngine(
                egm.Object,
                properties.Object,
                hostFactory.Object,
                deviceFactory.Object,
                scriptManager.Object,
                packageDownloadManager.Object,
                deviceObserver.Object,
                progressiveDeviceObserver.Object,
                egmStateObserver.Object,
                deviceRegistryService.Object,
                gatComponentFactory.Object,
                meterSubManager.Object,
                g2sMeterProvider.Object,
                voucherDataService.Object,
                masterResetService.Object,
                clientStatus.Object,
                certificates.Object,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEmdiExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();
            var hostFactory = new Mock<IHostFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var scriptManager = new Mock<IScriptManager>();
            var packageDownloadManager = new Mock<IPackageDownloadManager>();
            var deviceObserver = new Mock<IDeviceObserver>();
            var progressiveDeviceObserver = new Mock<IProgressiveDeviceObserver>();
            var egmStateObserver = new Mock<IEgmStateObserver>();
            var deviceRegistryService = new Mock<IDeviceRegistryService>();
            var gatComponentFactory = new Mock<IGatComponentFactory>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var g2sMeterProvider = new Mock<IG2SMeterProvider>();
            var voucherDataService = new Mock<IVoucherDataService>();
            var masterResetService = new Mock<IMasterResetService>();
            var clientStatus = new Mock<ISelfTest>();
            var certificates = new Mock<ICertificateService>();
            var certificateMonitor = new Mock<ICertificateMonitor>();

            var service = new G2SEngine(
                egm.Object,
                properties.Object,
                hostFactory.Object,
                deviceFactory.Object,
                scriptManager.Object,
                packageDownloadManager.Object,
                deviceObserver.Object,
                progressiveDeviceObserver.Object,
                egmStateObserver.Object,
                deviceRegistryService.Object,
                gatComponentFactory.Object,
                meterSubManager.Object,
                g2sMeterProvider.Object,
                voucherDataService.Object,
                masterResetService.Object,
                clientStatus.Object,
                certificates.Object,
                certificateMonitor.Object,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCentralServiceExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();
            var hostFactory = new Mock<IHostFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var scriptManager = new Mock<IScriptManager>();
            var packageDownloadManager = new Mock<IPackageDownloadManager>();
            var deviceObserver = new Mock<IDeviceObserver>();
            var progressiveDeviceObserver = new Mock<IProgressiveDeviceObserver>();
            var egmStateObserver = new Mock<IEgmStateObserver>();
            var deviceRegistryService = new Mock<IDeviceRegistryService>();
            var gatComponentFactory = new Mock<IGatComponentFactory>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var g2sMeterProvider = new Mock<IG2SMeterProvider>();
            var voucherDataService = new Mock<IVoucherDataService>();
            var masterResetService = new Mock<IMasterResetService>();
            var clientStatus = new Mock<ISelfTest>();
            var certificates = new Mock<ICertificateService>();
            var certificateMonitor = new Mock<ICertificateMonitor>();
            var emdi = new Mock<IEmdi>();

            var service = new G2SEngine(
                egm.Object,
                properties.Object,
                hostFactory.Object,
                deviceFactory.Object,
                scriptManager.Object,
                packageDownloadManager.Object,
                deviceObserver.Object,
                progressiveDeviceObserver.Object,
                egmStateObserver.Object,
                deviceRegistryService.Object,
                gatComponentFactory.Object,
                meterSubManager.Object,
                g2sMeterProvider.Object,
                voucherDataService.Object,
                masterResetService.Object,
                clientStatus.Object,
                certificates.Object,
                certificateMonitor.Object,
                emdi.Object,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        public void WhenStopExpectEgmStopped()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();
            var hostFactory = new Mock<IHostFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var scriptManager = new Mock<IScriptManager>();
            var packageDownloadManager = new Mock<IPackageDownloadManager>();
            var deviceObserver = new Mock<IDeviceObserver>();
            var progressiveDeviceObserver = new Mock<IProgressiveDeviceObserver>();
            var egmStateObserver = new Mock<IEgmStateObserver>();
            var deviceRegistryService = new Mock<IDeviceRegistryService>();
            var gatComponentFactory = new Mock<IGatComponentFactory>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var g2sMeterProvider = new Mock<IG2SMeterProvider>();
            var voucherDataService = new Mock<IVoucherDataService>();
            var masterResetService = new Mock<IMasterResetService>();
            var clientStatus = new Mock<ISelfTest>();
            var certificates = new Mock<ICertificateService>();
            var certificateMonitor = new Mock<ICertificateMonitor>();
            var emdi = new Mock<IEmdi>();
            var central = new Mock<ICentralService>();

            var service = new G2SEngine(
                egm.Object,
                properties.Object,
                hostFactory.Object,
                deviceFactory.Object,
                scriptManager.Object,
                packageDownloadManager.Object,
                deviceObserver.Object,
                progressiveDeviceObserver.Object,
                egmStateObserver.Object,
                deviceRegistryService.Object,
                gatComponentFactory.Object,
                meterSubManager.Object,
                g2sMeterProvider.Object,
                voucherDataService.Object,
                masterResetService.Object,
                clientStatus.Object,
                certificates.Object,
                certificateMonitor.Object,
                emdi.Object,
                central.Object);

            service.Stop();

            egm.Verify(e => e.Stop());
        }
    }
}
