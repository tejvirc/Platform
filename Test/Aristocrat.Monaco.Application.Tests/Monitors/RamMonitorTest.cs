namespace Aristocrat.Monaco.Application.Tests.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Monitors;

    using Contracts;
	using Hardware.Contracts;
	using Hardware.Contracts.Audio;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    ///     This is a test class for RamMonitorTest and is intended
    ///     to contain all RamMonitorTest Unit Tests
    /// </summary>
    [TestClass]
    public sealed class RamMonitorTest
    {
        private Mock<ISystemDisableManager> _disableManager;
        private Mock<IEventBus> _eventBus;
        private Mock<IPersistentStorageManager> _storage;
        private Mock<IMeterManager> _meterManager;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IAudio> _audioService;

        private RamMonitor _target;

        /// <summary>Initializes class members and prepares for execution of a TestMethod.</summary>
        [TestInitialize]
        public void Initialize()
        {
            _disableManager = new Mock<ISystemDisableManager>();
            _eventBus = new Mock<IEventBus>();
            _storage = new Mock<IPersistentStorageManager>();
            _meterManager = new Mock<IMeterManager>();
            _propertiesManager = new Mock<IPropertiesManager>();
            _audioService = new Mock<IAudio>();
            _propertiesManager.Setup(
                    m => m.GetProperty(
                        ApplicationConstants.PeriodicCriticalMemoryIntegrityCheckEnabled,
                        It.IsAny<bool>()))
                .Returns(false);
            _propertiesManager.Setup(
                    m => m.GetProperty(ApplicationConstants.PeriodicCriticalMemoryIntegrityCheckValue, It.IsAny<int>()))
                .Returns(100);

            _propertiesManager.Setup(
                    m => m.GetProperty(
                        ApplicationConstants.ReadOnlyMediaRequired,
                        It.IsAny<bool>()))
                .Returns(false);

            _storage.Setup(m => m.VerifyIntegrity(true)).Returns(true);

            _target = new RamMonitor(
                _eventBus.Object,
                _storage.Object,
                _propertiesManager.Object,
                _disableManager.Object,
                _meterManager.Object,
                _audioService.Object);
        }

        private void SetupAudio()
        {
            _audioService.Setup(p => p.Load());
            _propertiesManager.Setup(p => p.GetProperty(HardwareConstants.AlertVolumeKey, It.IsAny<object>())).Returns((byte)10);
        }

        [TestCleanup]
        public void CleanUp()
        {
        }

        [TestMethod]
        public void RamMonitorConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        /// <summary>A test for Initialize.</summary>
        [TestMethod]
        public void InitializeTest()
        {
            MockSubscribeToEvents();

            _target.Initialize();

            _eventBus.VerifyAll();

            _storage.Verify(m => m.VerifyIntegrity(true), Times.Once);
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual("RamMonitor", _target.Name);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            Assert.AreEqual(1, _target.ServiceTypes.Count);
            Assert.IsTrue(_target.ServiceTypes.Contains(typeof(RamMonitor)));
        }

        [TestMethod]
        public void DisposeTest()
        {
            _eventBus.Setup(mock => mock.UnsubscribeAll(_target)).Verifiable();

            _target.Dispose();

            _eventBus.VerifyAll();
        }

        [TestMethod]
        public void WhenHandlePersistentStorageIntegrityCheckFailedEventExpectDisabled()
        {
            Action<PersistentStorageIntegrityCheckFailedEvent> handler = null;

            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PersistentStorageIntegrityCheckFailedEvent>>()))
                .Callback<object, Action<PersistentStorageIntegrityCheckFailedEvent>>((subscriber, callback) => handler = callback);

            SetupAudio();

            _target.Initialize();

            Assert.IsNotNull(handler);

            handler(new PersistentStorageIntegrityCheckFailedEvent());

            _disableManager.Verify(
                m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null));
            _audioService.Verify(
                m => m.Play(SoundName.None, It.IsAny<int>(), It.IsAny<float>(), SpeakerMix.All, null), Times.Never);
        }

        [TestMethod]
        public void ExpectAudioAlertWhenPersistentStorageIntegrityCheckFailedEvent()
        {
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.AlertVolumeKey, It.IsAny<byte>())).Returns((byte)5);

            Action<PersistentStorageIntegrityCheckFailedEvent> handler = null;

            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PersistentStorageIntegrityCheckFailedEvent>>()))
                .Callback<object, Action<PersistentStorageIntegrityCheckFailedEvent>>((subscriber, callback) => handler = callback);

            _target.Initialize();

            Assert.IsNotNull(handler);

            handler(new PersistentStorageIntegrityCheckFailedEvent());

            _disableManager.Verify(
                m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null));
            _audioService.Verify(
                m => m.Play(It.IsAny<SoundName>(), It.IsAny<int>(), It.IsAny<float>(), SpeakerMix.All, null), Times.Once);
        }

        [TestMethod]
        public void WhenHandlePersistentStorageClearStartedEventExpectEnabled()
        {
            Action<PersistentStorageClearStartedEvent> handler = null;

            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PersistentStorageClearStartedEvent>>()))
                .Callback<object, Action<PersistentStorageClearStartedEvent>>((subscriber, callback) => handler = callback);

            _target.Initialize();

            Assert.IsNotNull(handler);

            handler(new PersistentStorageClearStartedEvent(PersistenceLevel.Static));

            _disableManager.Verify(m => m.Enable(It.IsAny<Guid>()));
        }

        [TestMethod]
        public void WhenHandleStorageErrorEventExpectDisabled()
        {
            Action<StorageErrorEvent> handler = null;

            _disableManager.Setup(m => m.CurrentDisableKeys).Returns(new List<Guid>());

            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<StorageErrorEvent>>()))
                .Callback<object, Action<StorageErrorEvent>>((subscriber, callback) => handler = callback);

            SetupAudio();

            var meter = new Mock<IMeter>();

            meter.Setup(m => m.Increment(1));

            _meterManager.Setup(m => m.GetMeter(ApplicationMeters.MemoryErrorCount)).Returns(meter.Object);

            _target.Initialize();

            Assert.IsNotNull(handler);

            handler(new StorageErrorEvent(StorageError.ReadFailure));

            _disableManager.Verify(
                m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null));

            meter.Verify(m => m.Increment(1));
        }

        [TestMethod]
        public void WhenReadOnlyMediaIsNotRequiredSecondaryStorageErrorIsRaised()
        {
            _propertiesManager.Setup(
                    m => m.GetProperty(
                        ApplicationConstants.ReadOnlyMediaRequired,
                        It.IsAny<bool>()))
                .Returns(false);

            Action<SecondaryStorageErrorEvent> handler = null;

            _disableManager.Setup(m => m.CurrentDisableKeys).Returns(new List<Guid>());

            _disableManager.Setup(
                m => m.Disable(
                    ApplicationConstants.SecondaryStorageMediaConnectedKey,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    null)).Verifiable();

            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<SecondaryStorageErrorEvent>>()))
                .Callback<object, Action<SecondaryStorageErrorEvent>>((subscriber, callback) => handler = callback);

            SetupAudio();

            _target.Initialize();

            Assert.IsNotNull(handler);

            handler(new SecondaryStorageErrorEvent(SecondaryStorageError.NotExpectedButConnected));

            _disableManager.Verify();
        }

        [TestMethod]
        public void WhenReadOnlyMediaIsRequiredSecondaryStorageErrorNotRaised()
        {
            _propertiesManager.Setup(
                    m => m.GetProperty(
                        ApplicationConstants.ReadOnlyMediaRequired,
                        It.IsAny<bool>()))
                .Returns(true);

            Action<SecondaryStorageErrorEvent> handler = null;

            _disableManager.Setup(m => m.CurrentDisableKeys).Returns(new List<Guid>());

            _disableManager.Setup(
                m => m.Disable(
                    ApplicationConstants.SecondaryStorageMediaConnectedKey,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    null)).Throws(new Exception("Shouldn't raise secondary lockup if Read only media is supported")); ;

            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<SecondaryStorageErrorEvent>>()))
                .Callback<object, Action<SecondaryStorageErrorEvent>>((subscriber, callback) => handler = callback);

            _target.Initialize();

            Assert.IsNotNull(handler);

            handler(new SecondaryStorageErrorEvent(SecondaryStorageError.NotExpectedButConnected));

        }

        private void MockSubscribeToEvents()
        {
            _eventBus.Setup(
                    mock => mock.Subscribe(_target, It.IsAny<Action<PersistentStorageIntegrityCheckFailedEvent>>()))
                .Verifiable();
            _eventBus.Setup(mock => mock.Subscribe(_target, It.IsAny<Action<PersistentStorageClearStartedEvent>>()))
                .Verifiable();
            _eventBus.Setup(mock => mock.Subscribe(_target, It.IsAny<Action<StorageErrorEvent>>())).Verifiable();
        }
    }
}