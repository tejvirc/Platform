namespace Aristocrat.Monaco.G2S.Data.Tests.Profile
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Common.Storage;
    using Data.Model;
    using Data.Profile;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;

    [TestClass]
    public class ProfileServiceTest
    {
        private const int DeviceId = 1;

        private const string DeviceClass = "G2S_optionConfig";
        private Mock<IMonacoContextFactory> _contextFactoryMock;

        private Mock<IDevice> _deviceMock;

        private Mock<IProfileDataRepository> _profileDataRepository;

        [TestInitialize]
        public void Initialize()
        {
            _contextFactoryMock = new Mock<IMonacoContextFactory>();
            _profileDataRepository = new Mock<IProfileDataRepository>();
            _deviceMock = new Mock<IDevice>();

            _deviceMock.SetupGet(m => m.Id).Returns(DeviceId);
            _deviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenContextFactoryIsNullExpectException()
        {
            var service = new ProfileService(null, _profileDataRepository.Object);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenProfileDataRepositoryIsNullExpectException()
        {
            var service = new ProfileService(_contextFactoryMock.Object, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenExistsWithNullProfileExpectException()
        {
            var service = new ProfileService(
                _contextFactoryMock.Object,
                _profileDataRepository.Object);

            service.Exists((IDevice)null);
        }

        [TestMethod]
        public void WhenProfileNotExistsExpectFalse()
        {
            var service = new ProfileService(
                _contextFactoryMock.Object,
                _profileDataRepository.Object);

            _profileDataRepository
                .Setup(m => m.Get(It.IsAny<DbContext>(), DeviceClass, DeviceId))
                .Returns((ProfileData)null);

            var result = service.Exists(_deviceMock.Object);

            Assert.IsFalse(result);

            _contextFactoryMock.Verify(m => m.CreateDbContext(), Times.Once);
        }

        [TestMethod]
        public void WhenProfileNotExistsExpectTrue()
        {
            var service = new ProfileService(
                _contextFactoryMock.Object,
                _profileDataRepository.Object);

            _profileDataRepository
                .Setup(m => m.Get(It.IsAny<DbContext>(), DeviceClass, DeviceId))
                .Returns(new ProfileData());

            var result = service.Exists(_deviceMock.Object);

            Assert.IsTrue(result);

            _contextFactoryMock.Verify(m => m.CreateDbContext(), Times.Once);
        }

        [TestMethod]
        public void WhenGetAllProfilesWithProfilesExpectProfiles()
        {
            var service = new ProfileService(
                _contextFactoryMock.Object,
                _profileDataRepository.Object);

            _profileDataRepository
                .Setup(m => m.GetAll(It.IsAny<DbContext>()))
                .Returns(new List<ProfileData> { new ProfileData(), new ProfileData() }.AsQueryable());

            var profiles = service.GetAll();
            Assert.AreEqual(2, profiles.Count());

            _contextFactoryMock.Verify(m => m.CreateDbContext(), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenSaveWithNullProfileExpectException()
        {
            var service = new ProfileService(
                _contextFactoryMock.Object,
                _profileDataRepository.Object);

            service.Save((IDevice)null);
        }

        [TestMethod]
        public void WhenSaveNewProfileExpectSuccess()
        {
            var service = new ProfileService(
                _contextFactoryMock.Object,
                _profileDataRepository.Object);

            _profileDataRepository
                .Setup(m => m.Get(It.IsAny<DbContext>(), DeviceClass, DeviceId))
                .Returns((ProfileData)null);

            service.Save(_deviceMock.Object);

            _profileDataRepository.Verify(m => m.Add(It.IsAny<DbContext>(), It.IsAny<ProfileData>()), Times.Once);
            _contextFactoryMock.Verify(m => m.CreateDbContext(), Times.Once);
        }

        [TestMethod]
        public void WhenSaveOldProfileExpectSuccess()
        {
            var service = new ProfileService(
                _contextFactoryMock.Object,
                _profileDataRepository.Object);

            _profileDataRepository
                .Setup(m => m.Get(It.IsAny<DbContext>(), DeviceClass, DeviceId))
                .Returns(new ProfileData());

            service.Save(_deviceMock.Object);

            _profileDataRepository.Verify(m => m.Update(It.IsAny<DbContext>(), It.IsAny<ProfileData>()), Times.Once);
            _contextFactoryMock.Verify(m => m.CreateDbContext(), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDeleteWithNullProfileExpectException()
        {
            var service = new ProfileService(
                _contextFactoryMock.Object,
                _profileDataRepository.Object);

            service.Delete((IDevice)null);
        }

        [TestMethod]
        public void WhenDeleteWithNotNullProfileExpectSuccess()
        {
            var service = new ProfileService(
                _contextFactoryMock.Object,
                _profileDataRepository.Object);

            var profileData = new ProfileData();
            _profileDataRepository.Setup(m => m.Get(It.IsAny<DbContext>(), DeviceClass, DeviceId))
                .Returns(profileData);

            service.Delete(_deviceMock.Object);

            _contextFactoryMock.Verify(m => m.CreateDbContext(), Times.Once);
            _profileDataRepository.Verify(m => m.Get(It.IsAny<DbContext>(), DeviceClass, DeviceId), Times.Once);
            _profileDataRepository.Verify(m => m.Delete(It.IsAny<DbContext>(), profileData));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPopulateWithNullProfileExpectException()
        {
            var service = new ProfileService(
                _contextFactoryMock.Object,
                _profileDataRepository.Object);

            service.Populate((IDevice)null);
        }

        [TestMethod]
        public void WhenPopulateWithNotNullProfileExpectSuccess()
        {
            var service = new ProfileService(
                _contextFactoryMock.Object,
                _profileDataRepository.Object);

            var device = new DeviceStub
            {
                HostEnabled = true
            };

            var profileData = new ProfileData
            {
                DeviceId = DeviceId,
                ProfileType = DeviceClass,
                Data = JsonConvert.SerializeObject(device)
            };

            _profileDataRepository.Setup(m => m.Get(It.IsAny<DbContext>(), DeviceClass, DeviceId))
                .Returns(profileData);

            var deviceToPopulate = new DeviceStub
            {
                HostEnabled = false
            };

            service.Populate(deviceToPopulate);

            Assert.AreEqual(true, deviceToPopulate.HostEnabled);

            _contextFactoryMock.Verify(m => m.CreateDbContext(), Times.Once);
        }

        private class DeviceStub : IDevice
        {
            public DeviceStub()
            {
                DeviceClass = ProfileServiceTest.DeviceClass;
                Id = DeviceId;
            }

            public string ConfigUpdatedEventCode { get; }

            public bool RequiredForPlay { get; }

            public string DisableText { get; set; }

            public ICommandQueue Queue { get; }

            public void Open(IStartupContext context)
            {
            }

            public void Close()
            {
            }

            public void RegisterEvents()
            {
            }

            public void UnregisterEvents()
            {
            }

            public int Id { get; }

            public bool Enabled { get; set; }

            public bool HostEnabled { get; set; }

            public long ConfigurationId { get; }

            public int Owner { get; set; }

            public int Configurator { get; }

            public IEnumerable<int> Guests { get; }

            public bool Locked { get; set; }

            public bool HostLocked { get; set; }

            public bool Active { get; set; }

            public bool UseDefaultConfig { get; }

            public string DeviceClass { get; }

            public string DevicePrefix { get; }

            public DateTime ConfigDateTime { get; }

            public bool ConfigComplete { get; }

            public DateTime? ListStateDateTime { get; }

            public bool Existing { get; }

            public void ApplyOptions(DeviceOptionConfigValues optionConfigValues)
            {
            }
        }
    }
}