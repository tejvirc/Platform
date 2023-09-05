namespace Aristocrat.Monaco.Application.Tests.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Monitors;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Application.Contracts.FirmwareCrcMonitor;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Hardware.Contracts.Audio;
    using Aristocrat.Monaco.Hardware.Contracts.ButtonDeck;
    using Aristocrat.Monaco.Hardware.Contracts.Door;
    using Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor;
    using Aristocrat.Monaco.Hardware.Contracts.Printer;
    using Aristocrat.Monaco.Kernel.Contracts;
    using Aristocrat.Monaco.Test.Common;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    ///     This is a test class for FirmwareCrcMonitorTest and is intended
    ///     to contain all FirmwareCrcMonitorTest Unit Tests
    /// </summary>
    [TestClass]
    public sealed class FirmwareCrcMonitorTest
    {
        private Mock<IServiceManager> _serviceManager;
        private Mock<ISystemDisableManager> _disableManager;
        private Mock<IEventBus> _eventBus;
        private Mock<IPersistentStorageManager> _storage;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IPersistentStorageTransaction> _transaction;
        private Mock<INoteAcceptor> _noteAcceptor;
        private Mock<IPrinter> _printer;
        private Mock<IButtonDeckDisplay> _buttonDeckDisplay;
        private Mock<IPersistentStorageAccessor> _storageAccessor;
        private Mock<IAudio> _audioService;
        private Func<ClosedEvent, CancellationToken, Task> _logicDoorEventHandle;
        private Func<PlatformBootedEvent, CancellationToken, Task> _platformBootedHandler;
        private FirmwareCrcMonitor _target;
        private IDictionary<int, Dictionary<string, object>> _getAllReturnValue;
        private int _noteAcceptorDesignatedCrc;
        private int _printerDesignatedCrc;
        private int _buttonDeckDesignatedCrc;
        /// <summary>Initializes class members and prepares for execution of a TestMethod.</summary>
        [TestInitialize]
        public void Initialize()
        {
            _serviceManager = MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _disableManager = new Mock<ISystemDisableManager>();
            _eventBus = new Mock<IEventBus>();
            _storage = new Mock<IPersistentStorageManager>();
            _propertiesManager = new Mock<IPropertiesManager>();
            _storageAccessor = new Mock<IPersistentStorageAccessor>();
            _transaction = new Mock<IPersistentStorageTransaction>();
            _audioService = new Mock<IAudio>();
            _getAllReturnValue = new Dictionary<int, Dictionary<string, object>>();
            var rng = new Random();
            _noteAcceptorDesignatedCrc = rng.Next();
            _printerDesignatedCrc = rng.Next();
            _buttonDeckDesignatedCrc = rng.Next();
        }

        [TestCleanup]
        public void CleanUp()
        {
            _getAllReturnValue.Clear();
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(true)]
        [DataRow(false)]
        [DataTestMethod]
        public void ConstructorTest(bool blockExists)
        {
            _storage.Setup(s => s.BlockExists(It.IsAny<string>())).Returns(blockExists);
            _target = new FirmwareCrcMonitor(
                _eventBus.Object,
                _storage.Object,
                _propertiesManager.Object,
                _disableManager.Object,
                _audioService.Object
            );
            Assert.IsNotNull(_target);
        }

        /// <summary>A test for Initialize where the block that persists data doesn't exist yet.</summary>
        [DataRow(false, false, false)]
        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        [DataRow(false, true, true)]
        [DataRow(true, false, true)]
        [DataRow(true, true, false)]
        [DataRow(true, true, true)]
        [DataTestMethod]
        public async Task InitializeBlockDoesntExistTest(bool bnaNull, bool printerNull, bool buttonDeckNull)
        {
            SetupInitializeBlockDoesntExist();
            SetupDevices(bnaNull, printerNull, buttonDeckNull);
            SetupDeviceCrcs(_noteAcceptorDesignatedCrc, _printerDesignatedCrc, _buttonDeckDesignatedCrc);

            _target.Initialize();
            await _platformBootedHandler.Invoke(new PlatformBootedEvent(), new CancellationToken());
            _eventBus.VerifyAll();
            _storage.VerifyAll();
            _disableManager.VerifyAll();
            _propertiesManager.VerifyAll();
        }

        /// <summary>Close the logic door with no mismatched CRCs and expect no disable to EGM</summary>
        [DataRow(false, false, false)]
        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        [DataRow(false, true, true)]
        [DataRow(true, false, true)]
        [DataRow(true, true, false)]
        [DataRow(true, true, true)]
        [DataTestMethod]
        public async Task HandleLogicDoorClosed_ExpectNoDisable(bool bnaNull, bool printerNull, bool buttonDeckNull)
        {
            AddPersistedCrcEntry(0, _noteAcceptorDesignatedCrc, bnaNull);
            AddPersistedCrcEntry(1, _printerDesignatedCrc, printerNull);
            AddPersistedCrcEntry(2, _buttonDeckDesignatedCrc, buttonDeckNull);
            SetupInitializeBlockExists();
            SetupDevices(bnaNull, printerNull, buttonDeckNull);
            SetupDeviceCrcs(_noteAcceptorDesignatedCrc, _printerDesignatedCrc, _buttonDeckDesignatedCrc);
            _target.Initialize();
            await _platformBootedHandler.Invoke(new PlatformBootedEvent(), new CancellationToken());
            _disableManager.SetupGet(d => d.CurrentDisableKeys).Returns(new List<Guid> { });
            var evt = new ClosedEvent((int)DoorLogicalId.Logic, "Unit Logic Door");
            await _logicDoorEventHandle.Invoke(evt, new CancellationToken());
            _disableManager.VerifyAll();
        }

        /// <summary>Close the logic door with mismatched CRCs and expect disable to EGM</summary>
        [DataRow(false, false, false)]
        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        [DataRow(false, true, true)]
        [DataRow(true, false, true)]
        [DataRow(true, true, false)]
        [DataTestMethod]
        public async Task HandleLogicDoorClosed_ExpectDisable(bool bnaNull, bool printerNull, bool buttonDeckNull)
        {
            AddPersistedCrcEntry(0, _noteAcceptorDesignatedCrc - 1, bnaNull);
            AddPersistedCrcEntry(1, _printerDesignatedCrc - 1, printerNull);
            AddPersistedCrcEntry(2, _buttonDeckDesignatedCrc - 1, buttonDeckNull);
            SetupPropertiesForAudioTest();
            SetupInitializeBlockExists();
            SetupDevices(bnaNull, printerNull, buttonDeckNull);
            SetupDeviceCrcs(_noteAcceptorDesignatedCrc, _printerDesignatedCrc, _buttonDeckDesignatedCrc);
            _target.Initialize();
            SetupAudioService();
            SetupLocalizer();

            _eventBus.Setup(mock => mock.Publish(new FirmwareCrcMismatchedEvent(It.IsAny<string>()))).Verifiable();

            await _platformBootedHandler.Invoke(new PlatformBootedEvent(), new CancellationToken());
            _disableManager.SetupGet(d => d.CurrentDisableKeys).Returns(new List<Guid> { });
            _disableManager.Setup(d => d.Disable(ApplicationConstants.MonitorSignatureMismatchDisableKey, SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), It.IsAny<Type>())).Verifiable();

            var evt = new ClosedEvent((int)DoorLogicalId.Logic, "Unit Logic Door");
            await _logicDoorEventHandle.Invoke(evt, new CancellationToken());

            _disableManager.Verify();
            _audioService.Verify();
            _eventBus.Verify(mock => mock.Publish(It.IsAny<FirmwareCrcMismatchedEvent>()), Times.Exactly(2));
        }

        /// <summary>Close the logic door with mismatched CRC and the EGM already disabled due to mismatched CRC and expect the CRCs to be persisted and the EGM re-enabled.</summary>
        [DataRow(false, false, false)]
        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        [DataRow(false, true, true)]
        [DataRow(true, false, true)]
        [DataRow(true, true, false)]
        [DataTestMethod]
        public async Task HandleLogicDoorCloseWhenAlreadyDisabledByMismatchedCRC_ExpectRemovedDisableAndPersistedCrc(bool bnaNull, bool printerNull, bool buttonDeckNull)
        {
            AddPersistedCrcEntry(0, _noteAcceptorDesignatedCrc - 1, bnaNull);
            AddPersistedCrcEntry(1, _printerDesignatedCrc - 1, printerNull);
            AddPersistedCrcEntry(2, _buttonDeckDesignatedCrc - 1, buttonDeckNull);
            SetupInitializeBlockExists();
            SetupDevices(bnaNull, printerNull, buttonDeckNull);
            SetupDeviceCrcs(_noteAcceptorDesignatedCrc, _printerDesignatedCrc, _buttonDeckDesignatedCrc);
            _target.Initialize();
            SetupLocalizer();
            await _platformBootedHandler.Invoke(new PlatformBootedEvent(), new CancellationToken());
            _disableManager.SetupGet(d => d.CurrentDisableKeys).Returns(new List<Guid> { ApplicationConstants.MonitorSignatureMismatchDisableKey });
            _disableManager.Setup(d => d.Enable(ApplicationConstants.MonitorSignatureMismatchDisableKey)).Verifiable();

            if (!bnaNull)
                _transaction.SetupSet(s => s[0, "Crc"] = _noteAcceptorDesignatedCrc).Verifiable();
            if (!printerNull)
                _transaction.SetupSet(s => s[1, "Crc"] = _printerDesignatedCrc).Verifiable();
            if (!buttonDeckNull)
                _transaction.SetupSet(s => s[2, "Crc"] = _buttonDeckDesignatedCrc).Verifiable();

            var evt = new ClosedEvent((int)DoorLogicalId.Logic, "Unit Logic Door");
            await _logicDoorEventHandle.Invoke(evt, new CancellationToken());
            _disableManager.Verify();
            _storageAccessor.Verify();
        }

        [TestMethod]
        public void NameTest()
        {
            _target = new FirmwareCrcMonitor(
                _eventBus.Object,
                _storage.Object,
                _propertiesManager.Object,
                _disableManager.Object,
                _audioService.Object
            );
            Assert.AreEqual("FirmwareCrcMonitor", _target.Name);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            _target = new FirmwareCrcMonitor(
                _eventBus.Object,
                _storage.Object,
                _propertiesManager.Object,
                _disableManager.Object,
                _audioService.Object
            );
            Assert.AreEqual(1, _target.ServiceTypes.Count);
            Assert.IsTrue(_target.ServiceTypes.Contains(typeof(FirmwareCrcMonitor)));
        }

        [TestMethod]
        public void DisposeTest()
        {
            _target = new FirmwareCrcMonitor(
                _eventBus.Object,
                _storage.Object,
                _propertiesManager.Object,
                _disableManager.Object,
                _audioService.Object
            );
            _eventBus.Setup(mock => mock.UnsubscribeAll(_target)).Verifiable();
            _target.Dispose();
            _eventBus.VerifyAll();
        }

        private void SetupInitializeCommon(bool setupInspect = true)
        {
            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.FirmwareCrcMonitorEnabled, It.IsAny<bool>())).Returns(true);
            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.FirmwareCrcMonitorSeed, It.IsAny<int>())).Returns(1);
            if (setupInspect)
            {
                _propertiesManager.Setup(p => p.GetProperty(KernelConstants.IsInspectionOnly, It.IsAny<bool>())).Returns(false);
            }
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Func<ClosedEvent, CancellationToken, Task>>(), It.IsAny<Predicate<ClosedEvent>>()))
                .Callback<object, Func<ClosedEvent, CancellationToken, Task>, Predicate<ClosedEvent>>((subscriber, callback, p) => _logicDoorEventHandle = callback);
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Func<PlatformBootedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<PlatformBootedEvent, CancellationToken, Task>>((subscriber, callback) => _platformBootedHandler = callback);
            _disableManager.Setup(d => d.Disable(ApplicationConstants.MonitorVerifyingDisableKey, SystemDisablePriority.Immediate,
            It.IsAny<Func<string>>(), It.IsAny<Type>())).Verifiable();
            _disableManager.Setup(d => d.Enable(ApplicationConstants.MonitorVerifyingDisableKey)).Verifiable();
            _storage.Setup(s => s.GetBlock(It.IsAny<string>())).Returns(_storageAccessor.Object);
        }

        private void SetupInitializeBlockDoesntExist()
        {
            _storage.Setup(s => s.BlockExists(It.IsAny<string>())).Returns(false);
            _storage.Setup(s => s.CreateBlock(PersistenceLevel.Static, It.IsAny<string>(), It.IsAny<int>())).Returns(_storageAccessor.Object);
            _storageAccessor.Setup(m => m.StartTransaction()).Returns(_transaction.Object);
            _transaction.Setup(s => s.Commit()).Verifiable();
            _target = new FirmwareCrcMonitor(
                _eventBus.Object,
                _storage.Object,
                _propertiesManager.Object,
                _disableManager.Object,
                _audioService.Object
            );
            SetupInitializeCommon(false);
        }

        private void AddPersistedCrcEntry(int blockPosition, int value, bool exitWithoutAdding)
        {
            // NoteAcceptor = 0,
            // Printer = 1,
            // LCDButtonDeck = 2,
            if (exitWithoutAdding)
            {
                return;
            }
            var dict = new Dictionary<string, object>();
            dict.Add("Crc", value);
            _getAllReturnValue.Add(blockPosition, dict);
        }

        private void SetupInitializeBlockExists()
        {
            _storage.Setup(s => s.BlockExists(typeof(FirmwareCrcMonitor).ToString())).Returns(true);
            _storageAccessor.Setup(s => s.GetAll()).Returns(_getAllReturnValue);
            _storageAccessor.Setup(m => m.StartTransaction()).Returns(_transaction.Object);
            _transaction.Setup(s => s.Commit()).Verifiable();
            _target = new FirmwareCrcMonitor(
                _eventBus.Object,
                _storage.Object,
                _propertiesManager.Object,
                _disableManager.Object,
                _audioService.Object
            );
            SetupInitializeCommon();
        }

        private void SetupDevices(bool bnaNull, bool printerNull, bool buttonDeckNull)
        {
            _noteAcceptor = bnaNull ? null : MoqServiceManager.CreateAndAddService<INoteAcceptor>(MockBehavior.Strict);
            if (bnaNull)
            {
                _serviceManager.Setup(s => s.TryGetService<INoteAcceptor>()).Returns((INoteAcceptor)null);
            }

            _printer = printerNull ? null : MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);
            if (printerNull)
            {
                _serviceManager.Setup(s => s.TryGetService<IPrinter>()).Returns((IPrinter)null);
            }

            _buttonDeckDisplay = buttonDeckNull ? null : MoqServiceManager.CreateAndAddService<IButtonDeckDisplay>(MockBehavior.Strict);
            if (buttonDeckNull)
            {
                _serviceManager.Setup(s => s.TryGetService<IButtonDeckDisplay>()).Returns((IButtonDeckDisplay)null);
            }
        }

        private void SetupLocalizer()
        {
            Mock<ILocalizer> _localizer = new Mock<ILocalizer>();
            _localizer.Setup(l => l.FormatString(It.IsAny<string>(), It.IsAny<string>())).Returns(It.IsAny<string>());

            Mock<ILocalizerFactory> _localizerFactory = new Mock<ILocalizerFactory>();
            _localizerFactory.Setup(l => l.For(It.IsAny<string>())).Returns(_localizer.Object);

            _serviceManager.Setup(s => s.GetService<ILocalizerFactory>()).Returns(_localizerFactory.Object);
        }

        private void SetupDeviceCrcs(int bnaCrc, int printerCrc, int buttonDeckCrc)
        {
            _noteAcceptor?.Setup(n => n.CalculateCrc(It.IsAny<int>())).Returns(Task.FromResult(bnaCrc));
            if (_noteAcceptor != null)
            {
                _noteAcceptor.SetupGet(n => n.Crc).Returns(bnaCrc);
            }

            _printer?.Setup(n => n.CalculateCrc(It.IsAny<int>())).Returns(Task.FromResult(printerCrc));
            if (_printer != null)
            {
                _printer.SetupGet(n => n.Crc).Returns(printerCrc);
            }

            _buttonDeckDisplay?.Setup(n => n.CalculateCrc(It.IsAny<int>())).Returns(Task.FromResult(buttonDeckCrc));
            if (_buttonDeckDisplay != null)
            {
                _buttonDeckDisplay.SetupGet(n => n.Crc).Returns(buttonDeckCrc);
            }
        }

        private void SetupAudioService()
        {
            _audioService.Setup(audio => audio.Play(It.IsAny<SoundName>(), It.IsAny<int>(), It.IsAny<float?>(), It.IsAny<SpeakerMix>(), null)).Verifiable();
        }

        private void SetupPropertiesForAudioTest()
        {
            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.FirmwareCrcErrorSoundKey, It.IsAny<string>())).Returns("test");
            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.AlertVolumeKey, It.IsAny<byte>())).Returns(It.IsAny<byte>());
        }
    }
}