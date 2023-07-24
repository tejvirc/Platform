namespace Aristocrat.Monaco.G2S.Tests.Security
{
    using System;
    using System.Threading;
    using Application.Contracts.Protocol;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Emdi;
    using Aristocrat.Monaco.G2S.Services;
    using Common.CertificateManager;
    using Common.CertificateManager.Models;
    using G2S.Meters;
    using G2S.Security;
    using G2S.Services;
    using G2S.Services.Progressive;
    using Hardware.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class CertificateMonitorTest
    {
        private Mock<ICertificateService> _certificateServiceMock;
        private Mock<IEventBus> _eventBusMock;
        private Mock<G2SEngine> _g2sEngineMock;

        [TestInitialize]
        public void Initialize()
        {
            _eventBusMock = new Mock<IEventBus>();
            _certificateServiceMock = new Mock<ICertificateService>();
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();
            var hostFactory = new Mock<IHostFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var scriptManager = new Mock<IScriptManager>();
            var packageDownloadManager = new Mock<IPackageDownloadManager>();
            var deviceObserver = new Mock<IDeviceObserver>();
            var progressiveDeviceManager = new Mock<IProgressiveDeviceManager>();
            var egmStateObserver = new Mock<IEgmStateObserver>();
            var deviceRegistryService = new Mock<IDeviceRegistryService>();
            var gatComponentFactory = new Mock<IGatComponentFactory>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var g2sMeterProvider = new Mock<IG2SMeterProvider>();
            var voucherDataService = new Mock<IVoucherDataService>();
            var masterResetService = new Mock<IMasterResetService>();
            var selfTest = new Mock<ISelfTest>();
            var certificateService = new Mock<ICertificateService>();
            var certificateMonitor = new Mock<ICertificateMonitor>();
            var emdi = new Mock<IEmdi>();
            var central = new Mock<ICentralService>();
            var eventLift = new Mock<IEventLift>();
            var multiProtocolConfigurationProvider = new Mock<IMultiProtocolConfigurationProvider>();

            _g2sEngineMock = new Mock<G2SEngine>(
                egm.Object,
                properties.Object,
                hostFactory.Object,
                deviceFactory.Object,
                scriptManager.Object,
                packageDownloadManager.Object,
                deviceObserver.Object,
                progressiveDeviceManager.Object,
                egmStateObserver.Object,
                deviceRegistryService.Object,
                gatComponentFactory.Object,
                meterSubManager.Object,
                g2sMeterProvider.Object,
                voucherDataService.Object,
                masterResetService.Object,
                selfTest.Object,
                certificateService.Object,
                certificateMonitor.Object,
                emdi.Object,
                central.Object,
                eventLift.Object,
                multiProtocolConfigurationProvider.Object);
            MoqServiceManager.AddService(_g2sEngineMock);
        }

        [TestMethod]
        public void WhenInitializeCertificateMonitorExpectSuccess()
        {
            var certificateMonitor = new CertificateMonitor(_certificateServiceMock.Object, _eventBusMock.Object);
            certificateMonitor.Start();
        }

        [TestMethod]
        public void WhenRunWithConfigurationIsNullExpectNoActions()
        {
            _certificateServiceMock.Setup(m => m.GetConfiguration()).Returns((PkiConfiguration)null);

            var certificateStatusResult = new GetCertificateStatusResult(
                CertificateStatus.Good,
                DateTime.UtcNow,
                null,
                DateTime.UtcNow);
            _certificateServiceMock.Setup(m => m.GetCertificateStatus()).Returns(certificateStatusResult);

            var certificateMonitor = new CertificateMonitor(_certificateServiceMock.Object, _eventBusMock.Object);
            certificateMonitor.Start();

            Thread.Sleep(1500);

            _eventBusMock.Verify(m => m.Publish(It.IsAny<CertificateStatusUpdatedEvent>()), Times.Never);
        }

        [TestMethod]
        public void WhenRunWithConfigurationOcspDisabledExpectNoActions()
        {
            var pkiConfiguration = new PkiConfiguration { OcspEnabled = false };

            _certificateServiceMock.Setup(m => m.GetConfiguration()).Returns(pkiConfiguration);

            var certificateStatusResult = new GetCertificateStatusResult(
                CertificateStatus.Good,
                DateTime.UtcNow,
                null,
                DateTime.UtcNow);
            _certificateServiceMock.Setup(m => m.GetCertificateStatus()).Returns(certificateStatusResult);

            var certificateMonitor = new CertificateMonitor(_certificateServiceMock.Object, _eventBusMock.Object);
            certificateMonitor.Start();

            Thread.Sleep(1500);

            _eventBusMock.Verify(m => m.Publish(It.IsAny<CertificateStatusUpdatedEvent>()), Times.Never);
        }
    }
}