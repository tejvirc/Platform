namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using System.Linq;
    using Application.Contracts;
    using Gaming.Runtime;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts;
    using Kernel.Contracts.Components;
    using Kernel.Contracts.Events;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class SoftwareInstallerTests
    {
        private const string IndexBlock = @"Aristocrat.Monaco.Gaming.SoftwareInstaller.Index";
        private const string DataBlock = @"Aristocrat.Monaco.Gaming.SoftwareInstaller.Data";
        private Mock<IComponentRegistry> _components;
        private Mock<IEventBus> _eventBus;
        private Mock<IPathMapper> _pathMapper;
        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<IRuntimeProvider> _runtimeLoader;
        private SoftwareInstaller _softwareInstaller;

        [TestInitialize]
        public void Initialize()
        {
            _pathMapper = new Mock<IPathMapper>();
            _components = new Mock<IComponentRegistry>();
            _runtimeLoader = new Mock<IRuntimeProvider>();
            _eventBus = new Mock<IEventBus>();
            _persistentStorage = new Mock<IPersistentStorageManager>();
            _softwareInstaller = new SoftwareInstaller(
                _pathMapper.Object,
                _components.Object,
                _runtimeLoader.Object,
                _eventBus.Object,
                _persistentStorage.Object);

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPathMapperIsNullExpectException()
        {
            _softwareInstaller = new SoftwareInstaller(
                null,
                _components.Object,
                _runtimeLoader.Object,
                _eventBus.Object,
                _persistentStorage.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenComponentRegistryIsNullExpectException()
        {
            _softwareInstaller = new SoftwareInstaller(
                _pathMapper.Object,
                null,
                _runtimeLoader.Object,
                _eventBus.Object,
                _persistentStorage.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenRuntimeProviderIsNullExpectException()
        {
            _softwareInstaller = new SoftwareInstaller(
                _pathMapper.Object,
                _components.Object,
                null,
                _eventBus.Object,
                _persistentStorage.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventBusIsNullExpectException()
        {
            _softwareInstaller = new SoftwareInstaller(
                _pathMapper.Object,
                _components.Object,
                _runtimeLoader.Object,
                null,
                _persistentStorage.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPersistentStorageManagerIsNullExpectException()
        {
            _softwareInstaller = new SoftwareInstaller(
                _pathMapper.Object,
                _components.Object,
                _runtimeLoader.Object,
                _eventBus.Object,
                null);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            _softwareInstaller = new SoftwareInstaller(
                _pathMapper.Object,
                _components.Object,
                _runtimeLoader.Object,
                _eventBus.Object,
                _persistentStorage.Object);
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual("Aristocrat.Monaco.Gaming.SoftwareInstaller", _softwareInstaller.Name);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            Assert.AreEqual(1, _softwareInstaller.ServiceTypes.Count);
            Assert.AreEqual(typeof(ISoftwareInstaller), _softwareInstaller.ServiceTypes.ToArray()[0]);
        }

        [TestMethod]
        public void DeviceChangedTest()
        {
            Assert.IsTrue(_softwareInstaller.DeviceChanged);
        }

        [TestMethod]
        public void ExitActionTest()
        {
            Assert.AreEqual(ExitAction.Shutdown, _softwareInstaller.ExitAction);
        }

        [TestMethod]
        public void InitializeTest()
        {
            var storageAccessorIndexBlock = new Mock<IPersistentStorageAccessor>();
            _persistentStorage.Setup(mock => mock.BlockExists(IndexBlock)).Returns(true);
            _persistentStorage.Setup(mock => mock.CreateBlock(PersistenceLevel.Critical, IndexBlock, 1))
                .Returns(storageAccessorIndexBlock.Object);
            storageAccessorIndexBlock.Setup(mock => mock["BlockIndex"]).Returns(0);

            _persistentStorage.Setup(mock => mock.BlockExists(IndexBlock)).Returns(false);
            _persistentStorage.Setup(mock => mock.BlockExists(DataBlock)).Returns(false);
            _persistentStorage.Setup(
                    mock => mock.CreateBlock(PersistenceLevel.Critical, DataBlock, 100))
                .Returns(Factory_CreateMockLogData().Object);

            _softwareInstaller.Initialize();
        }

        [TestMethod]
        public void InstallNothingTest()
        {
            Assert.IsFalse(_softwareInstaller.Install(""));
        }

        [TestMethod]
        public void InstallRuntimeTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<MediaAlteredEvent>())).Verifiable();
            Assert.IsTrue(_softwareInstaller.Install("ATI_Runtime_Fake"));

            _eventBus.Verify();
        }

        [TestMethod]
        public void InstallPlatformTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<MediaAlteredEvent>())).Verifiable();
            Assert.IsTrue(_softwareInstaller.Install("ATI_Platform_Fake"));

            _eventBus.Verify();
        }

        [TestMethod]
        public void UninstallNothingTest()
        {
            Assert.IsFalse(_softwareInstaller.Uninstall(""));
        }

        [TestMethod]
        public void UninstallRuntimeTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<MediaAlteredEvent>())).Verifiable();
            Assert.IsTrue(_softwareInstaller.Uninstall("ATI_Runtime_Fake"));

            _eventBus.Verify();
        }

        [TestMethod]
        public void UninstallPlatformTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<MediaAlteredEvent>())).Verifiable();
            Assert.IsTrue(_softwareInstaller.Uninstall("ATI_Platform_Fake"));

            _eventBus.Verify();
        }

        private static Mock<IPersistentStorageAccessor> Factory_CreateMockLogData(int count = 1)
        {
            var storageAccessorDataBlock = new Mock<IPersistentStorageAccessor>();
            storageAccessorDataBlock.Setup(m => m[It.IsInRange(0, count, Range.Inclusive), "FileName"])
                .Returns("ATI_FakeGame.iso");
            storageAccessorDataBlock.Setup(m => m[It.IsInRange(0, count, Range.Inclusive), "Active"]).Returns(false);
            return storageAccessorDataBlock;
        }
    }
}