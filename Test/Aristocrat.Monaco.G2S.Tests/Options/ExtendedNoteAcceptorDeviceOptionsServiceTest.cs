namespace Aristocrat.Monaco.G2S.Tests.Options
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.Monaco.Kernel.Contracts;
    using Data.Model;
    using G2S.Options;
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class ExtendedNoteAcceptorDeviceOptionsServiceTest
    {
        private const int ConfigurationId = 123;

        private Mock<IDeviceObserver> _deviceObserverMock = new Mock<IDeviceObserver>();
        private Mock<INoteAcceptor> _noteAcceptorServiceMock = new Mock<INoteAcceptor>();
        private Mock<IPersistenceProvider> _persistence = new Mock<IPersistenceProvider>();
        private Mock<IPropertiesManager> _property = new Mock<IPropertiesManager>();
        private Mock<IDeviceRegistryService> _registry;
        private Mock<IPersistentStorageManager> _storage;

        [TestInitialize]
        public void Initialize()
        {
            _deviceObserverMock = new Mock<IDeviceObserver>();
            _noteAcceptorServiceMock = new Mock<INoteAcceptor>();

            _registry = new Mock<IDeviceRegistryService>();
            _registry.Setup(m => m.GetDevice<INoteAcceptor>()).Returns(_noteAcceptorServiceMock.Object);

            _storage = new Mock<IPersistentStorageManager>();
            _noteAcceptorServiceMock.Setup(m => m.GetSupportedNotes(It.IsAny<string>()))
                .Returns(
                    new Collection<int>()
                    {
                        1,
                        5,
                        10,
                        20,
                        50,
                        100
                    });
            _storage.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(true);
            _persistence.Setup(a => a.GetOrCreateBlock(It.IsAny<string>(), It.IsAny<PersistenceLevel>())).Returns(new Mock<IPersistentBlock>().Object);
        }

        [TestMethod]
        public void WhenMatchesExpectSuccess()
        {
            var service = new NoteAcceptorDeviceOptions(_registry.Object, _persistence.Object, _property.Object);

            Assert.IsTrue(service.Matches(DeviceClass.NoteAcceptor));
            Assert.IsFalse(service.Matches(DeviceClass.Gat));
        }

        [TestMethod]
        public void WhenNoteEnabledIsFalseExpectSuccess()
        {
            var denoms = new List<int>
            {
                1,
                5,
                10,
                50,
                100
            };
            _property.Setup(m => m.SetProperty(PropertyKey.VoucherIn, It.IsAny<bool>())).Verifiable();
            _noteAcceptorServiceMock.Setup(m => m.UpdateDenom(It.IsAny<int>(), It.IsAny<bool>())).Callback(() => denoms.Clear());

                _noteAcceptorServiceMock.SetupAllProperties();
            _noteAcceptorServiceMock.SetupGet(m => m.Denominations).Returns(denoms);

            ConfigureDeviceRegistryService();

            var service = new NoteAcceptorDeviceOptions(_registry.Object, _persistence.Object, _property.Object);

            var deviceOptionConfigValues = new DeviceOptionConfigValues(ConfigurationId);
            deviceOptionConfigValues.AddOption("G2S_noteEnabled", "false");
            deviceOptionConfigValues.AddOption("G2S_voucherEnabled", "false");

            service.ApplyProperties(CreateDevice(), deviceOptionConfigValues);
            _property.Verify();
            Assert.AreEqual(denoms.Count, 0);
        }

        [TestMethod]
        public void WhenNoteEnabledIsTrueAndVoucherEnabledIsFalseExpectSuccess()
        {
            var denoms = new List<int>
            {
                1,
                5,
                10,
                50,
                100
            };
            _noteAcceptorServiceMock.Setup(m => m.UpdateDenom(It.IsAny<int>(), It.IsAny<bool>()));
            _property.Setup(m => m.SetProperty(PropertyKey.VoucherIn, It.IsAny<bool>())).Verifiable();
            _noteAcceptorServiceMock.SetupAllProperties();
            _noteAcceptorServiceMock.SetupGet(m => m.Denominations).Returns(denoms);
            ConfigureDeviceRegistryService();

            var service = new NoteAcceptorDeviceOptions(_registry.Object, _persistence.Object, _property.Object);

            var deviceOptionConfigValues = new DeviceOptionConfigValues(ConfigurationId);
            deviceOptionConfigValues.AddOption("G2S_noteEnabled", "true");
            deviceOptionConfigValues.AddOption("G2S_voucherEnabled", "false");

            service.ApplyProperties(CreateDevice(), deviceOptionConfigValues);
            _property.Verify();
            Assert.AreEqual(denoms.Count, 5);
        }

        [TestMethod]
        public void WhenNoteEnabledIsTrueAndDenominationsEqualOneExpectSuccess()
        {
            var denoms = new List<int>
            {
                1,
                5,
                10,
                50,
                100
            };
            _noteAcceptorServiceMock.Setup(m => m.UpdateDenom(It.IsAny<int>(), It.IsAny<bool>()));
            _property.Setup(m => m.SetProperty(PropertyKey.VoucherIn, It.IsAny<bool>())).Verifiable();
            ConfigureDeviceRegistryService();

            _noteAcceptorServiceMock.SetupGet(x => x.Denominations)
                .Returns(denoms);

            var service = new NoteAcceptorDeviceOptions(_registry.Object, _persistence.Object, _property.Object);

            var deviceOptionConfigValues = new DeviceOptionConfigValues(ConfigurationId);
            deviceOptionConfigValues.AddOption("G2S_noteEnabled", "true");
            deviceOptionConfigValues.AddOption("G2S_voucherEnabled", "false");

            service.ApplyProperties(CreateDevice(), deviceOptionConfigValues);
            _property.Verify();
            Assert.AreEqual(denoms.Count, 5);
        }

        [TestMethod]
        public void WhenVoucherEnabledIsFalseAndNoteEnabledTrueExpectSuccess()
        {
            var denoms = new List<int>
            {
                1,
                5,
                10,
                50,
                100
            };
            _noteAcceptorServiceMock.Setup(m => m.UpdateDenom(It.IsAny<int>(), It.IsAny<bool>()));
            _noteAcceptorServiceMock.SetupAllProperties();
            _noteAcceptorServiceMock.SetupGet(x => x.Denominations).Returns(denoms);
            ConfigureDeviceRegistryService();
            _property.Setup(m => m.SetProperty(PropertyKey.VoucherIn, It.IsAny<bool>())).Verifiable();
            var service = new NoteAcceptorDeviceOptions(_registry.Object, _persistence.Object, _property.Object);

            var deviceOptionConfigValues = new DeviceOptionConfigValues(ConfigurationId);
            deviceOptionConfigValues.AddOption("G2S_voucherEnabled", "false");
            deviceOptionConfigValues.AddOption("G2S_noteEnabled", "true");

            service.ApplyProperties(CreateDevice(), deviceOptionConfigValues);
            _property.Verify();
            Assert.AreEqual(denoms.Count, 5);
        }

        [TestMethod]
        public void WhenVoucherEnabledIsFalseAndDenominationsEqualNoVoucherExpectSuccess()
        {
            var denoms = new List<int>
            {
                1,
                5,
                10,
                50,
                100
            };
            _noteAcceptorServiceMock.Setup(m => m.UpdateDenom(It.IsAny<int>(), It.IsAny<bool>()));
            ConfigureDeviceRegistryService();
            _property.Setup(m => m.SetProperty(PropertyKey.VoucherIn, It.IsAny<bool>())).Verifiable();
            _noteAcceptorServiceMock.SetupAllProperties();
            _noteAcceptorServiceMock.SetupGet(x => x.Denominations).Returns(denoms);

            var service = new NoteAcceptorDeviceOptions(_registry.Object, _persistence.Object, _property.Object);

            var deviceOptionConfigValues = new DeviceOptionConfigValues(ConfigurationId);
            deviceOptionConfigValues.AddOption("G2S_noteEnabled", "true");
            deviceOptionConfigValues.AddOption("G2S_voucherEnabled", "false");

            service.ApplyProperties(CreateDevice(), deviceOptionConfigValues);
            _property.Verify();
            Assert.AreEqual(denoms.Count, 5);
        }

        [TestMethod]
        public void WhenVoucherEnabledIsFalseAndDenominationsEqualNoneExpectSuccess()
        {
            var denoms = new List<int>
            {
                1,
                5,
                10,
                50,
                100
            };

            _noteAcceptorServiceMock.Setup(m => m.UpdateDenom(It.IsAny<int>(), It.IsAny<bool>())).Callback(() => denoms.Clear());

            ConfigureDeviceRegistryService();

            _noteAcceptorServiceMock.SetupAllProperties();
            _noteAcceptorServiceMock.SetupGet(x => x.Denominations).Returns(denoms);
            _property.Setup(m => m.SetProperty(PropertyKey.VoucherIn, It.IsAny<bool>())).Verifiable();
            var service = new NoteAcceptorDeviceOptions(_registry.Object, _persistence.Object, _property.Object);

            var deviceOptionConfigValues = new DeviceOptionConfigValues(ConfigurationId);
            deviceOptionConfigValues.AddOption("G2S_noteEnabled", "false");
            deviceOptionConfigValues.AddOption("G2S_voucherEnabled", "false");

            service.ApplyProperties(CreateDevice(), deviceOptionConfigValues);
            _property.Verify();
            Assert.AreEqual(denoms.Count, 0);
        }



        private NoteAcceptorDevice CreateDevice()
        {
            return new NoteAcceptorDevice(1, _deviceObserverMock.Object);
        }

        private void ConfigureDeviceRegistryService()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.AddService(_noteAcceptorServiceMock);
        }
    }
}
