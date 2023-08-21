/*namespace Aristocrat.Monaco.G2S.Tests
{
    using System;
    using Accounting.Contracts;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using G2S.Services;
    using Gaming.Contracts;
    using Gaming.Contracts.Jackpot;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class HostFactoryTest
    {
        private const int HostId = 7;
        private static readonly Uri HostUrl = new Uri("http://localhost/");
        private readonly G2SJackpotSourceFactory _jackpotSourceFactory = new G2SJackpotSourceFactory();

        private Mock<IG2SEgm> _egm;
        private JackpotProvider _jackpotProvider;

        [TestInitialize]
        public void InitializeTest()
        {
            _egm = new Mock<IG2SEgm>();
            _jackpotProvider = new JackpotProvider(
                new Mock<IJackpotBroker>().Object,
                _egm.Object,
                new Mock<ITransactionHistory>().Object,
                new Mock<IGameProvider>().Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEgmIsNullExpectException()
        {
            var factory = new HostFactory(null, null, null, null, null, null, null, null);

            Assert.IsNull(factory);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDeviceFactoryIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var factory = new HostFactory(egm.Object, null, null, null, null, null, null, null);

            Assert.IsNull(factory);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenStateObserverIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var devices = new Mock<IDeviceFactory>();

            var factory = new HostFactory(egm.Object, devices.Object, null, null, null, null, null, null);

            Assert.IsNull(factory);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventPersistenceIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var devices = new Mock<IDeviceFactory>();
            var observer = new Mock<ITransportStateObserver>();

            var factory = new HostFactory(egm.Object, devices.Object, observer.Object, null, null, null, null, null);

            Assert.IsNull(factory);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var devices = new Mock<IDeviceFactory>();
            var observer = new Mock<ITransportStateObserver>();
            var commsObserver = new Mock<ICommunicationsStateObserver>();
            var eventPersistence = new Mock<IEventPersistenceManager>();
            var deviceObserver = new Mock<IDeviceObserver>();

            var factory = new HostFactory(
                _egm.Object,
                devices.Object,
                observer.Object,
                commsObserver.Object,
                deviceObserver.Object,
                eventPersistence.Object,
                _jackpotProvider,
                _jackpotSourceFactory);

            Assert.IsNotNull(factory);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreateWithNullHostExpectException()
        {
            var devices = new Mock<IDeviceFactory>();
            var observer = new Mock<ITransportStateObserver>();
            var commsObserver = new Mock<ICommunicationsStateObserver>();
            var eventPersistence = new Mock<IEventPersistenceManager>();
            var deviceObserver = new Mock<IDeviceObserver>();

            var factory = new HostFactory(
                _egm.Object,
                devices.Object,
                observer.Object,
                commsObserver.Object,
                deviceObserver.Object,
                eventPersistence.Object,
                _jackpotProvider,
                _jackpotSourceFactory);

            factory.Create(null);
        }

        [TestMethod]
        public void WhenCreateExpectRegistered()
        {
            var devices = new Mock<IDeviceFactory>();
            var observer = new Mock<ITransportStateObserver>();
            var commsObserver = new Mock<ICommunicationsStateObserver>();
            var eventPersistence = new Mock<IEventPersistenceManager>();
            var deviceObserver = new Mock<IDeviceObserver>();

            var host = new Mock<IHost>();
            host.SetupGet(h => h.Id).Returns(HostId);
            host.SetupGet(h => h.Index).Returns(HostId);
            host.SetupGet(h => h.Address).Returns(HostUrl);

            var factory = new HostFactory(
                _egm.Object,
                devices.Object,
                observer.Object,
                commsObserver.Object,
                deviceObserver.Object,
                eventPersistence.Object,
                _jackpotProvider,
                _jackpotSourceFactory);

            factory.Create(host.Object);

            _egm.Verify(e => e.RegisterHost(HostId, HostUrl, HostId));

            //TODO:  Verify device creation
        }
    }
}*/