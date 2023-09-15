namespace Aristocrat.Monaco.Application.Tests.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO.Packaging;
    using System.Linq;
    using System.Threading;
    using Application.Monitors;
    using Contracts;
    using Contracts.EdgeLight;
    using Contracts.Localization;
    using Kernel.Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.EdgeLighting;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     This is a test class for DoorMonitor and is intended
    ///     to contain all DoorMonitor Unit Tests.
    /// </summary>
    [TestClass]
    public class DoorMonitorTest
    {
        /// <summary>
        ///     Logical Ids for various doors
        /// </summary>
        private const int MainDoorId = 5;
        private const int BellyDoorId = 6;
        private const int LogicDoorId = 7;
        private const int TopBoxDoorId = 12;
        private const int UniversalInterfaceBoxDoorId = 13;
        private const int CashDoorId = 23;
        private const int SecondaryCashDoorId = 24;
        private const int DropDoorId = 25;
        private const int MechanicalMeterDoorId = 26;
        private const int MainOpticDoorId = 27;
        private const int TopBoxOpticDoorId = 28;

        /// <summary>Name of persistent storage block containing door events log.</summary>
        private static readonly string BlockNameLog = typeof(DoorMonitor) + ".Log";

        /// <summary>Name of persistent storage block containing door events index.</summary>
        private static readonly string BlockNameIndex = typeof(DoorMonitor) + ".Index";

        /// <summary>Name of persistent storage block holding information about last known state of doors.</summary>
        private static readonly string BlockNameStates = typeof(DoorMonitor) + ".States";

        private readonly AutoResetEvent _listener = new AutoResetEvent(false);

        /// <summary>A collection of fake implementation of IMeter.</summary>
        private readonly Dictionary<string, Mock<IMeter>> _meters = new Dictionary<string, Mock<IMeter>>
        {
            { "BellyDoorOpen", new Mock<IMeter>() },
            { "BellyDoorOpenPowerOff", new Mock<IMeter>() },
            { "CashDoorOpen", new Mock<IMeter>() },
            { "CashDoorOpenPowerOff", new Mock<IMeter>() },
            { "LogicDoorOpen", new Mock<IMeter>() },
            { "LogicDoorOpenPowerOff", new Mock<IMeter>() },
            { "MainDoorOpen", new Mock<IMeter>() },
            { "MainDoorOpenPowerOff", new Mock<IMeter>() },
            { "SecondaryCashDoorOpen", new Mock<IMeter>() },
            { "SecondaryCashDoorOpenPowerOff", new Mock<IMeter>() },
            { "TopBoxDoorOpen", new Mock<IMeter>() },
            { "TopBoxDoorOpenPowerOff", new Mock<IMeter>() },
            { "DropDoorOpen", new Mock<IMeter>() },
            { "DropDoorOpenPowerOff", new Mock<IMeter>() },
            { "MechanicalMeterDoorOpen", new Mock<IMeter>() },
            { "MechanicalMeterDoorOpenPowerOff", new Mock<IMeter>() },
            { "MainOpticDoorOpen", new Mock<IMeter>() },
            { "MainOpticDoorOpenPowerOff", new Mock<IMeter>() },
            { "TopBoxOpticDoorOpen", new Mock<IMeter>() },
            { "TopBoxOpticDoorOpenPowerOff", new Mock<IMeter>() }
        };

        private dynamic _accessor;

        /// <summary>
        ///     Tracks how many messages are received as a message consumer.
        /// </summary>
        private List<DoorMonitorAppendedEventArgs> _appendedMessages;

        private Mock<ISystemDisableManager> _disableManager;
        private Mock<IEdgeLightingStateManager> _edgeLightingStateManager;

        /// <summary>
        ///     Tracks how many messages are displayed;
        /// </summary>
        private List<string> _displayedMessages;

        private Mock<IDoorService> _doorService;
        private Mock<IEventBus> _eventBus;
        private Mock<INoteAcceptor> _noteAcceptor;
        private Mock<IOperatorMenuLauncher> _operatorMenuLauncher;

        /// <summary>
        ///     Tracks how many door events are handled.
        /// </summary>
        private List<BlockEntry> _eventQueue;

        private Mock<IIO> _iio;

        /// <summary>This is what gets read for IsDoorOpenBitMask from mocked persistent storage accessor</summary>
        private long _isDoorOpenBitMask;

        /// <summary>This is what gets read for IsMeteredBitMask from mocked persistent storage accessor</summary>
        private long _isMeteredBitMask;

        /// <summary>This is what gets read for IsVerificationTicketQueuedBitMask from mocked persistent storage accessor</summary>
        private long _isVerificationTicketQueuedBitMask;

        /// <summary>This is what gets read for the door meters lifetime value from mocked persistent storage accessor</summary>
        private long _lifeTimeCount;

        private Mock<IMessageDisplay> _messageDisplay;
        private Mock<IMeterManager> _meterManager;
        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<IPropertiesManager> _propertiesManager;

        /// <summary>This is what gets read for MessageRedisplayBitMask from mocked persistent storage accessor</summary>
        private long _redisplayBitMask;

        /// <summary>
        ///     Tracks how many messages are removed;
        /// </summary>
        private List<string> _removedMessages;

        private Mock<IPersistentStorageAccessor> _storageAccessor;
        private Mock<IPersistentStorageAccessor> _storageAccessorForStates;

        private DoorMonitor _target;
        private Mock<IAudio> _audioService;

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            PackUriHelper.Create(new Uri("reliable://0"));

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _storageAccessor = new Mock<IPersistentStorageAccessor>();
            _storageAccessorForStates = new Mock<IPersistentStorageAccessor>();

            _persistentStorage = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);

            _storageAccessorForStates.SetupSet(m => m["MessageRedisplayBitMask"] = It.IsAny<long>())
                .Callback<string, object>(
                    (name, bitmask) => _redisplayBitMask = (long)bitmask);
            _storageAccessorForStates.SetupSet(m => m["IsDoorOpenBitMask"] = It.IsAny<long>()).Callback<string, object>(
                (name, bitmask) => _isDoorOpenBitMask = (long)bitmask);
            _storageAccessorForStates.SetupSet(m => m["IsMeteredBitMask"] = It.IsAny<long>()).Callback<string, object>(
                (name, bitmask) => _isMeteredBitMask = (long)bitmask);
            _storageAccessorForStates.SetupSet(m => m["IsVerificationTicketQueuedBitMask"] = It.IsAny<long>())
                .Callback<string, object>(
                    (name, bitmask) => _isVerificationTicketQueuedBitMask = (long)bitmask);

            foreach (KeyValuePair<string, Mock<IMeter>> pair in _meters)
            {
                _storageAccessorForStates.SetupSet(m => m[pair.Key] = It.IsAny<long>()).Callback<string, object>(
                    (name, count) => _lifeTimeCount = (long)count);
            }

            _storageAccessorForStates.SetupGet(m => m["MessageRedisplayBitMask"]).Returns(() => _redisplayBitMask);
            _storageAccessorForStates.SetupGet(m => m["IsDoorOpenBitMask"]).Returns(() => _isDoorOpenBitMask);
            _storageAccessorForStates.SetupGet(m => m["IsMeteredBitMask"]).Returns(() => _isMeteredBitMask);
            _storageAccessorForStates.SetupGet(m => m["IsVerificationTicketQueuedBitMask"])
                .Returns(() => _isVerificationTicketQueuedBitMask);

            foreach (KeyValuePair<string, Mock<IMeter>> pair in _meters)
            {
                _storageAccessorForStates.SetupGet(m => m[pair.Key]).Returns(() => _lifeTimeCount);
            }

            _persistentStorage.Setup(m => m.BlockExists(BlockNameStates)).Returns(false);
            _persistentStorage.Setup(m => m.BlockExists(BlockNameIndex)).Returns(false);
            _persistentStorage.Setup(m => m.CreateBlock(PersistenceLevel.Critical, BlockNameStates, 1))
                .Returns(_storageAccessorForStates.Object)
                .Verifiable();

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _messageDisplay = MoqServiceManager.CreateAndAddService<IMessageDisplay>(MockBehavior.Strict);
            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);

            _disableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Strict);
            _doorService = MoqServiceManager.CreateAndAddService<IDoorService>(MockBehavior.Strict);
            _edgeLightingStateManager =
                MoqServiceManager.CreateAndAddService<IEdgeLightingStateManager>(MockBehavior.Strict);
            _noteAcceptor = MoqServiceManager.CreateAndAddService<INoteAcceptor>(MockBehavior.Strict);
            _operatorMenuLauncher = MoqServiceManager.CreateAndAddService<IOperatorMenuLauncher>(MockBehavior.Strict);

            _operatorMenuLauncher.Setup(m => m.IsShowing).Returns(true);

            _iio = MoqServiceManager.CreateAndAddService<IIO>(MockBehavior.Strict);
            _iio.Setup(m => m.SetMechanicalMeterLight(true)).Verifiable();
            _iio.Setup(m => m.SetMechanicalMeterLight(false)).Verifiable();
            _iio.Setup(m => m.GetQueuedEvents).Returns(new Collection<IEvent>());

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty(It.IsAny<string>(), It.IsAny<long>())).Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty(It.IsAny<string>(), It.IsAny<int>())).Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.AlertVolumeKey, It.IsAny<byte>())).Returns((byte)100);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.LockupCulture, CultureFor.Operator)).Returns(CultureFor.Operator);

            SetupMeters();

            var doors = new Dictionary<int, LogicalDoor>
            {
                { MainDoorId, new LogicalDoor(49, "Main Door", "Main Door") },
                { BellyDoorId, new LogicalDoor(51, "Belly Door", "Belly Door") },
                { LogicDoorId, new LogicalDoor(45, "Logic Door", "Logic Door") },
                { TopBoxDoorId, new LogicalDoor(46, "Top Box Door", "Top Box Door") },
                { CashDoorId, new LogicalDoor(50, "Stacker Door", "Stacker Door") },
                { SecondaryCashDoorId, new LogicalDoor(32, "Secondary Stacker Door", "Secondary Stacker Door") },
                { DropDoorId, new LogicalDoor(48, "Drop Door", "Drop Door") },
                { MechanicalMeterDoorId, new LogicalDoor(52, "Mechanical Meter Door", "Mechanical Meter Door") },
                { MainOpticDoorId, new LogicalDoor(3, "Main Optic Door", "Main Optic Door") },
                { TopBoxOpticDoorId, new LogicalDoor(4, "Top Box Optic Door", "Top Box Optic Door") },
                { UniversalInterfaceBoxDoorId, new LogicalDoor(46, "Universal Interface Box Door", "Universal Interface Box Door") }
            };

            _doorService.Setup(m => m.LogicalDoors).Returns(doors);
            foreach (var pair in doors)
            {
                _doorService.Setup(m => m.GetDoorClosed(pair.Key)).Returns(doors[pair.Key].Closed);
            }

            _audioService = MoqServiceManager.CreateAndAddService<IAudio>(MockBehavior.Strict);
            
            _audioService.Setup(
                a => a.Play(It.IsAny<SoundName>(), It.IsAny<int>(), It.IsAny<float>(), It.IsAny<SpeakerMix>(), It.IsAny<Action>()));
            _audioService.Setup(a => a.Play(It.IsAny<SoundName>(), It.IsAny<int>(), It.IsAny<SpeakerMix>(), It.IsAny<Action>()));
            _audioService.Setup(a => a.Play(It.IsAny<SoundName>(), It.IsAny<float>(), It.IsAny<SpeakerMix>(), It.IsAny<Action>()));
            _audioService.Setup(m => m.GetVolume(It.IsAny<VolumeLevel>())).Returns(It.IsAny<float>());

            var logicalDoors = new Dictionary<int, LogicalDoor>();
            _doorService.Setup(mock => mock.LogicalDoors).Returns(logicalDoors);

            _target = new DoorMonitor();
            _target.InitializeServices();
            _accessor = new DynamicPrivateObject(_target);

            _accessor._storageAccessorForStates = _storageAccessorForStates.Object;
            SetupDoors();
            _accessor.CheckLogicalDoors();

            _eventQueue = new List<BlockEntry>();
            _displayedMessages = new List<string>();
            _removedMessages = new List<string>();
            _appendedMessages = new List<DoorMonitorAppendedEventArgs>();
        }

        [TestMethod]
        public void GetNumDoorsOpenNoDoorServiceTest()
        {
            MoqServiceManager.RemoveService<IDoorService>();
            Assert.AreEqual((uint)0, _accessor.GetNumDoorsOpen());
        }

        /// <summary>
        ///     A test for CheckDoorAlarm with one door open and door alarm disabled.
        /// </summary>
        [TestMethod]
        public void CheckDoorAlarmWithOneDoorOpenDoorAlarmDisabledTest()
        {
            var logicalDoors = new Dictionary<int, LogicalDoor>();
            var logicalDoor = new LogicalDoor { Closed = false };
            logicalDoors.Add(1, logicalDoor);
            _doorService.Setup(mock => mock.LogicalDoors).Returns(logicalDoors);
            _doorService.Setup(mock => mock.GetDoorClosed(1)).Returns(false);
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.DoorAlarmEnabledKey, It.IsAny<bool>())).Returns(false);

            _accessor.CheckDoorAlarm(true, false);
        }

        /// <summary>
        ///     A test for CheckDoorAlarm with one door open and no audio service.
        /// </summary>
        [TestMethod]
        public void CheckDoorAlarmWithOneDoorOpenNoAudioServiceTest()
        {
            var logicalDoors = new Dictionary<int, LogicalDoor>();
            var logicalDoor = new LogicalDoor { Closed = false };
            logicalDoors.Add(1, logicalDoor);
            _doorService.Setup(mock => mock.LogicalDoors).Returns(logicalDoors);
            _doorService.Setup(mock => mock.GetDoorClosed(1)).Returns(false);
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.DoorAlarmEnabledKey, It.IsAny<bool>())).Returns(true);

            MoqServiceManager.RemoveService<IAudio>();
            _accessor.CheckDoorAlarm(true, false);
        }

        /// <summary>
        ///     A test for CheckDoorAlarm with more than one door open and is boot.
        /// </summary>
        [TestMethod]
        public void CheckDoorAlarmWithMoreThanOneDoorOpenAndIsBootTest()
        {
            var logicalDoors = new Dictionary<int, LogicalDoor>();
            var logicalDoor = new LogicalDoor { Closed = false };
            logicalDoors.Add(1, logicalDoor);
            logicalDoors.Add(2, logicalDoor);
            _doorService.Setup(mock => mock.LogicalDoors).Returns(logicalDoors);
            _doorService.Setup(mock => mock.GetDoorClosed(1)).Returns(false);
            _doorService.Setup(mock => mock.GetDoorClosed(2)).Returns(false);
            _doorService.Setup(mock => mock.GetDoorOpen(7)).Returns(true);
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.DoorAlarmEnabledKey, It.IsAny<bool>())).Returns(true);
            _propertiesManager.Setup(p => p.GetProperty(HardwareConstants.SoundConfigurationLogicDoorFullVolumeAlert, It.IsAny<object>())).Returns(true);
            _propertiesManager.Setup(p => p.GetProperty(KernelConstants.IsInspectionOnly, It.IsAny<object>())).Returns(true);

            _accessor.CheckDoorAlarm(true, true);
        }

        /// <summary>
        ///     A test for CheckDoorAlarm with all doors closed.
        /// </summary>
        [TestMethod]
        public void CheckDoorAlarmWithAllDoorsClosedTest()
        {
            _audioService.Setup(a => a.IsPlaying()).Returns(true);
            _audioService.Setup(a => a.Stop());
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.DoorAlarmEnabledKey, It.IsAny<bool>())).Returns(true);

            var logicalDoors = new Dictionary<int, LogicalDoor>();
            _doorService.Setup(mock => mock.LogicalDoors).Returns(logicalDoors);

            _accessor.CheckDoorAlarm(false, false);
        }

        [TestMethod]
        public void OnDoorOpenAlarmTimeoutNoAudioServiceTest()
        {
            MoqServiceManager.RemoveService<IAudio>();
            _accessor.OnDoorOpenAlarmTimeout(null, null);
        }

        /// <summary>A test for StopOpenDoorAlarm with no audio service</summary>
        [TestMethod]
        public void StopOpenDoorAlarmNoAudioServiceTest()
        {
            MoqServiceManager.RemoveService<IAudio>();
            _accessor.StopOpenDoorAlarm();
        }

        [TestCleanup]
        [ExpectedException(typeof(NullReferenceException))]
        public void CleanUp()
        {
            _accessor = null;

            _listener.Close();

            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void ConstructorWithMessagesToRedisplayTest()
        {
            _persistentStorage.Setup(m => m.BlockExists(BlockNameStates)).Returns(true);
            _persistentStorage.Setup(m => m.GetBlock(BlockNameStates))
                .Returns(_storageAccessorForStates.Object)
                .Verifiable();

            // Test construction of list of door message to redisplay
            // Note: _redisplayBitMask is what gets read from mocked persistent storage accessor
            var bitMaskToGet = 0L;
            foreach (int logicalId in _accessor.Doors.Keys)
            {
                long setBit = 1 << logicalId;
                bitMaskToGet |= setBit;
            }

            _redisplayBitMask = bitMaskToGet;

            var doorMonitor = new DoorMonitor();
            _accessor = new DynamicPrivateObject(doorMonitor);

            Assert.AreEqual(_accessor.Doors.Count, _accessor._messagesToRedisplay.Count);
        }

        [TestMethod]
        public void ConstructorWithNullLogicalDoorsTest()
        {
            _doorService.Setup(m => m.LogicalDoors).Returns((Dictionary<int, LogicalDoor>)null);

            var monitor = new DoorMonitor();
            Assert.AreEqual(0, monitor.Doors.Count);
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.IsTrue(_target.Name.Equals("DoorMonitor"));
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            Assert.IsTrue(_target.ServiceTypes.Contains(typeof(IDoorMonitor)));
        }

        [TestMethod]
        public void LogTest()
        {
            var messages = new LinkedList<DoorMessage>();
            for (var i = 0; i < 3; ++i)
            {
                var message = new DoorMessage { DoorId = i };
                messages.AddLast(message);
            }

            _accessor._messages = messages;
            Assert.AreEqual(messages.Count, _target.Log.Count);
            foreach (var each in _target.Log)
            {
                Assert.IsTrue(messages.Contains(each));
            }
        }

        [TestMethod]
        public void DoorsTest()
        {
            var doors = new Dictionary<int, DoorMonitor.DoorInfo>
            {
                {
                    MainDoorId,
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.MainDoorGuid.ToString(),
                        ApplicationMeters.MainDoorOpenCount,
                        ApplicationMeters.MainDoorOpenPowerOffCount)
                },
                {
                    BellyDoorId,
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.BellyDoorGuid.ToString(),
                        ApplicationMeters.BellyDoorOpenCount,
                        ApplicationMeters.DropDoorOpenPowerOffCount)
                },
                {
                    LogicDoorId,
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.LogicDoorGuid.ToString(),
                        ApplicationMeters.LogicDoorOpenCount,
                        ApplicationMeters.LogicDoorOpenPowerOffCount)
                }
            };

            var doorsAccessor = new Dictionary<int, DoorMonitor.DoorInfo>
            {
                {
                    MainDoorId,
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.MainDoorGuid.ToString(),
                        ApplicationMeters.MainDoorOpenCount,
                        ApplicationMeters.MainDoorOpenPowerOffCount)
                },
                {
                    BellyDoorId,
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.BellyDoorGuid.ToString(),
                        ApplicationMeters.BellyDoorOpenCount,
                        ApplicationMeters.DropDoorOpenPowerOffCount)
                },
                {
                    LogicDoorId,
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.LogicDoorGuid.ToString(),
                        ApplicationMeters.LogicDoorOpenCount,
                        ApplicationMeters.LogicDoorOpenPowerOffCount)
                }
            };
            _accessor._doors = doors;
            Assert.AreEqual(doors.Count, _target.Doors.Count);

            foreach (var each in _target.Doors)
            {
                Assert.IsTrue(doors.Keys.Contains(each.Key));
                Assert.AreEqual(each.Value, _accessor.GetDoorString(doorsAccessor[each.Key]));
            }
        }

        [TestMethod]
        public void MaxStoredLogMessagesTest()
        {
            var expectedValue = 10;

            Assert.AreNotEqual(expectedValue, _target.MaxStoredLogMessages);

            _target.MaxStoredLogMessages = expectedValue;

            Assert.AreEqual(expectedValue, _accessor._maxStoredMessages);

            _target.MaxStoredLogMessages = -1;

            Assert.AreEqual(0, _accessor._maxStoredMessages);
        }

        [TestMethod]
        public void GetDoorStringTest()
        {
            Assert.AreEqual(
                "Belly Door",
                _accessor.GetDoorString(
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.BellyDoorGuid.ToString(),
                        ApplicationMeters.BellyDoorOpenCount,
                        ApplicationMeters.BellyDoorOpenPowerOffCount)));
            Assert.AreEqual(
                "Stacker Door",
                _accessor.GetDoorString(
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.CashDoorGuid.ToString(),
                        ApplicationMeters.CashDoorOpenCount,
                        ApplicationMeters.CashDoorOpenPowerOffCount)));
            Assert.AreEqual(
                "Logic Door",
                _accessor.GetDoorString(
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.LogicDoorGuid.ToString(),
                        ApplicationMeters.LogicDoorOpenCount,
                        ApplicationMeters.LogicDoorOpenPowerOffCount)));
            Assert.AreEqual(
                "Main Door",
                _accessor.GetDoorString(
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.MainDoorGuid.ToString(),
                        ApplicationMeters.MainDoorOpenCount,
                        ApplicationMeters.MainDoorOpenPowerOffCount)));
            Assert.AreEqual(
                "Top Box Door",
                _accessor.GetDoorString(
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.TopBoxDoorGuid.ToString(),
                        ApplicationMeters.TopBoxDoorOpenCount,
                        ApplicationMeters.TopBoxDoorOpenPowerOffCount)));
            Assert.AreEqual(
                "Drop Door",
                _accessor.GetDoorString(
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.DropDoorGuid.ToString(),
                        ApplicationMeters.DropDoorOpenCount,
                        ApplicationMeters.DropDoorOpenPowerOffCount)));
            Assert.AreEqual(
                "Mechanical Meter Door",
                _accessor.GetDoorString(
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.MechanicalMeterDoorGuid.ToString(),
                        ApplicationMeters.MechanicalMeterDoorOpenCount,
                        ApplicationMeters.MechanicalMeterDoorOpenPowerOffCount)));
            Assert.AreEqual(
                "Main Optic Door",
                _accessor.GetDoorString(
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.MainOpticDoorGuid.ToString(),
                        ApplicationMeters.MainOpticDoorOpenCount,
                        ApplicationMeters.MainOpticDoorOpenPowerOffCount)));
            Assert.AreEqual(
                "Top Box Optic Door",
                _accessor.GetDoorString(
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.TopBoxOpticDoorGuid.ToString(),
                        ApplicationMeters.TopBoxOpticDoorOpenCount,
                        ApplicationMeters.TopBoxOpticDoorOpenPowerOffCount)));
            Assert.AreEqual(
                "Universal Interface Box Door",
                _accessor.GetDoorString(
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.UniversalInterfaceBoxDoorGuid.ToString(),
                        ApplicationMeters.UniversalInterfaceBoxDoorOpenCount,
                        ApplicationMeters.UniversalInterfaceBoxDoorOpenPowerOffCount)));
            Assert.AreEqual(
                "Unknown Door",
                _accessor.GetDoorString(
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.DropDoorGuid.ToString(),
                        "UnknownDoorMeterName",
                        "UnknownDoorPowerOffMeterName")));
        }

        [TestMethod]
        public void DoubleMeterTest()
        {
            var testEvent = new OpenEvent(MainDoorId, true, "Main Door");

            _doorService.Setup(m => m.GetDoorClosed(MainDoorId)).Returns(false);
            MockLog(false, testEvent.LogicalId, testEvent.Timestamp, true, true);
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.MainDoorGuid,
                "Main Door is Open",
                "Main Door Closed");
            _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();
            _edgeLightingStateManager.Setup(x => x.SetState(EdgeLightState.DoorOpen)).Returns(null as IEdgeLightToken);
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.DoorAlarmEnabledKey, It.IsAny<bool>())).Returns(true);
            _accessor.CheckForPreviouslyOpenDoors();

            _accessor.HandleEvent(new OpenEvent(MainDoorId, "Main Door"));

            _storageAccessor.Verify();
        }

        [TestMethod]
        public void DoorOpenNotMeteredTest()
        {
            var testEvent = new OpenEvent(MainDoorId, true, "Main Door");

            _doorService.Setup(m => m.GetDoorClosed(MainDoorId)).Returns(false);
            MockLog(false, testEvent.LogicalId, testEvent.Timestamp, true, true);
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.MainDoorGuid,
                "Main Door is Open",
                "Main Door Closed");
            _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();
            _edgeLightingStateManager.Setup(x => x.SetState(EdgeLightState.DoorOpen)).Returns(null as IEdgeLightToken);

            // Setup mocked persistent storage to indicate that door is open
            var bitMaskToGet = 0L;
            long setBit = 1 << MainDoorId;
            bitMaskToGet |= setBit;
            _isDoorOpenBitMask = bitMaskToGet;
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.DoorAlarmEnabledKey, It.IsAny<bool>())).Returns(true);

            _accessor.HandleEvent(new OpenEvent(MainDoorId, "Main Door"));

            _storageAccessor.Verify();
        }

        [TestMethod]
        public void DoorOpenMeterAlreadyIncrementedTest()
        {
            var testEvent = new OpenEvent(MainDoorId, true, "Main Door");

            _doorService.Setup(m => m.GetDoorClosed(MainDoorId)).Returns(false);
            MockLog(false, testEvent.LogicalId, testEvent.Timestamp, true, true);
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.MainDoorGuid,
                "Main Door is Open",
                "Main Door Closed");
            _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();
            _edgeLightingStateManager.Setup(x => x.SetState(EdgeLightState.DoorOpen)).Returns(null as IEdgeLightToken);

            // Setup mocked persistent storage to indicate that door is open
            var bitMaskToGet = 0L;
            long setBit = 1 << MainDoorId;
            bitMaskToGet |= setBit;
            _isDoorOpenBitMask = bitMaskToGet;

            _lifeTimeCount = 1; // This will cover meter already incremented
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.DoorAlarmEnabledKey, It.IsAny<bool>())).Returns(true);

            _accessor.HandleEvent(new OpenEvent(MainDoorId, "Main Door"));

            _meterManager.Verify();
            _storageAccessor.Verify();
            _meters[ApplicationMeters.MainDoorOpenCount].Verify();
        }

        [TestMethod]
        public void DoorOpenAndMeteredNoVerificationTicketTest()
        {
            var testEvent = new OpenEvent(LogicDoorId, true, "Logic Door");

            _doorService.Setup(m => m.GetDoorClosed(LogicDoorId)).Returns(false);
            MockLog(false, testEvent.LogicalId, testEvent.Timestamp, true, true);
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.LogicDoorGuid,
                "Logic Door is Open",
                "Logic Door Closed");
            _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();
            _edgeLightingStateManager.Setup(x => x.SetState(EdgeLightState.DoorOpen)).Returns(null as IEdgeLightToken);
            // Setup mocked persistent storage to indicate that door is open and metered
            var bitMaskToGet = 0L;
            long setBit = 1 << LogicDoorId;
            bitMaskToGet |= setBit;
            _isDoorOpenBitMask = bitMaskToGet;
            _isMeteredBitMask = bitMaskToGet;
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.DoorAlarmEnabledKey, It.IsAny<bool>())).Returns(true);

            _accessor.SetOrClearIsVerificationTicketQueuedBit(LogicDoorId, true); // For coverage
            _isVerificationTicketQueuedBitMask = 0L; // Turn off for test

            _accessor._queuedOpenEventCount = 1; // For delay posting of DoorOpenMeteredEvent coverage

            _accessor.HandleEvent(new OpenEvent(LogicDoorId, "Logic Door"));

            _meterManager.Verify();
            _storageAccessor.Verify();
            _meters[ApplicationMeters.LogicDoorOpenCount].Verify();
        }

        /////// <summary>
        ///////     A test for ProcessDoorEvents.  The logic, main, cash, and top box doors are opened via
        ///////     events and only the logic and main doors are closed via events.
        /////// </summary>
        ////[TestMethod]
        ////public void ProcessDoorEventManyEventTest()
        ////{
        ////    // Test the multiple open events.
        ////    var cashDoorOpenEvent = new OpenEvent(CashDoorId, "Cash Door");
        ////    var logicDoorOpenEvent = new OpenEvent(LogicDoorId, "Logic Door");
        ////    var mainDoorOpenEvent = new OpenEvent(MainDoorId, "Main Door");
        ////    var topBoxDoorOpenEvent = new OpenEvent(TopBoxDoorId, "Top Box Door");

        ////    MockLog(true, cashDoorOpenEvent.LogicalId, cashDoorOpenEvent.Timestamp, true, true);
        ////    MockLog(true, logicDoorOpenEvent.LogicalId, logicDoorOpenEvent.Timestamp, true, true);
        ////    MockLog(true, mainDoorOpenEvent.LogicalId, mainDoorOpenEvent.Timestamp, true, true);
        ////    MockLog(true, topBoxDoorOpenEvent.LogicalId, topBoxDoorOpenEvent.Timestamp, true, true);

        ////    _meters[ApplicationMeters.CashDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();
        ////    _meters[ApplicationMeters.LogicDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();
        ////    _meters[ApplicationMeters.TopBoxDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();

        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.CashDoorGuid,
        ////        "Cash Door is Open,
        ////        "Cash Door Closed");
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.LogicDoorGuid,
        ////        "Logic Door is Open",
        ////        "Logic Door Closed");
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.MainDoorGuid,
        ////        "Main Door is Open",
        ////        "Main Door Closed");
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.TopBoxDoorGuid,
        ////        "Top Box Door is Open",
        ////        "Top Box Door Closed");

        ////    _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();
        ////    _messageDisplay.Setup(m => m.RemoveMessage(It.IsAny<DisplayableMessage>())).Verifiable();

        ////    _accessor.HandleEvent(cashDoorOpenEvent);
        ////    _accessor.HandleEvent(logicDoorOpenEvent);
        ////    _accessor.HandleEvent(mainDoorOpenEvent);
        ////    _accessor.HandleEvent(topBoxDoorOpenEvent);

        ////    _messageDisplay.Verify();
        ////    Assert.AreEqual(4, _displayedMessages.Count);

        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.CashDoorGuid), _displayedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.LogicDoorGuid), _displayedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.MainDoorGuid), _displayedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.TopBoxDoorGuid), _displayedMessages));

        ////    Assert.AreEqual(4, _removedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.CashDoorGuid), _removedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.LogicDoorGuid), _removedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.MainDoorGuid), _removedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.TopBoxDoorGuid), _removedMessages));

        ////    _meters[ApplicationMeters.CashDoorOpenCount].Verify(m => m.Increment(1), Times.Exactly(1));
        ////    _meters[ApplicationMeters.LogicDoorOpenCount].Verify(m => m.Increment(1), Times.Exactly(1));
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Verify(m => m.Increment(1), Times.Exactly(1));
        ////    _meters[ApplicationMeters.TopBoxDoorOpenCount].Verify(m => m.Increment(1), Times.Exactly(1));

        ////    // Test the multiple closed events.
        ////    var logicDoorClosedEvent = new ClosedEvent(LogicDoorId, "Logic Door");
        ////    var mainDoorClosedEvent = new ClosedEvent(MainDoorId, "Main Door");

        ////    MockDisableManagerAndMessageDisplay(
        ////        false,
        ////        _accessor.LogicDoorGuid,
        ////        "Logic Door is Open",
        ////        "Logic Door Closed");
        ////    MockDisableManagerAndMessageDisplay(
        ////        false,
        ////        _accessor.MainDoorGuid,
        ////        "Main Door is Open",
        ////        "Main Door Closed");

        ////    MockLog(true, logicDoorClosedEvent.LogicalId, logicDoorOpenEvent.Timestamp, true, true);
        ////    MockLog(true, mainDoorClosedEvent.LogicalId, mainDoorOpenEvent.Timestamp, true, true);
        ////    _persistentStorage.Setup(m => m.VerifyIntegrity(true)).Returns(true).Verifiable();
        ////    _iio.Setup(m => m.ResetPhysicalDoorWasOpened(It.IsAny<int>()));

        ////    _accessor.HandleEvent(new ClosedEvent(LogicDoorId, "Logic Door"));
        ////    _accessor.HandleEvent(new ClosedEvent(MainDoorId, "Main Door"));

        ////    Assert.AreEqual(6, _displayedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.CashDoorGuid), _displayedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.LogicDoorGuid), _displayedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.MainDoorGuid), _displayedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.TopBoxDoorGuid), _displayedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.LogicDoorGuid), _displayedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.MainDoorGuid), _displayedMessages));

        ////    Assert.AreEqual(6, _removedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.CashDoorGuid), _removedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.LogicDoorGuid), _removedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.MainDoorGuid), _removedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.TopBoxDoorGuid), _removedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.LogicDoorGuid), _removedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.MainDoorGuid), _removedMessages));

        ////    // Meters are not updated for the closed events.
        ////    _meters[ApplicationMeters.CashDoorOpenCount].Verify(m => m.Increment(1), Times.Exactly(1));
        ////    _meters[ApplicationMeters.LogicDoorOpenCount].Verify(m => m.Increment(1), Times.Exactly(1));
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Verify(m => m.Increment(1), Times.Exactly(1));
        ////    _meters[ApplicationMeters.TopBoxDoorOpenCount].Verify(m => m.Increment(1), Times.Exactly(1));
        ////}

        ////[TestMethod]
        ////public void HandleDoorOpenBellyDoorTest()
        ////{
        ////    _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();

        ////    var theEvent = new OpenEvent(BellyDoorId, "Belly Door");
        ////    MockLog(false, theEvent.LogicalId, theEvent.Timestamp, true, true);
        ////    _meters[ApplicationMeters.BellyDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.BellyDoorGuid,
        ////        "Belly Door is Open",
        ////        "Belly Door Closed");

        ////    _accessor.HandleEvent(theEvent);

        ////    Assert.IsTrue(_accessor._logicalDoorsLoaded);

        ////    Assert.AreEqual(1, _displayedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.BellyDoorGuid), _displayedMessages));

        ////    Assert.AreEqual(1, _removedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.BellyDoorGuid), _removedMessages));

        ////    _meters[ApplicationMeters.BellyDoorOpenCount].Verify(m => m.Increment(1), Times.Once());
        ////    _meters[ApplicationMeters.BellyDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////}

        ////[TestMethod]
        ////public void HandleDoorOpenCashDoorTest()
        ////{
        ////    _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();

        ////    var theEvent = new OpenEvent(CashDoorId, "Cash Door");
        ////    MockLog(false, theEvent.LogicalId, theEvent.Timestamp, true, true);
        ////    _meters[ApplicationMeters.CashDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.CashDoorGuid,
        ////        "Cash Door is Open,
        ////        "Cash Door Closed");

        ////    _accessor.HandleEvent(theEvent);

        ////    Assert.AreEqual(1, _displayedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.CashDoorGuid), _displayedMessages));

        ////    Assert.AreEqual(1, _removedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.CashDoorGuid), _removedMessages));

        ////    _meters[ApplicationMeters.BellyDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.BellyDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenCount].Verify(m => m.Increment(1), Times.Once());
        ////    _meters[ApplicationMeters.CashDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////}

        ////[TestMethod]
        ////public void HandleDoorOpenLogicDoorTest()
        ////{
        ////    _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();

        ////    var theEvent = new OpenEvent(LogicDoorId, "Logic Door");
        ////    MockLog(false, theEvent.LogicalId, theEvent.Timestamp, true, true);
        ////    _meters[ApplicationMeters.LogicDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.LogicDoorGuid,
        ////        "Logic Door is Open",
        ////        "Logic Door Closed");

        ////    _accessor.HandleEvent(theEvent);

        ////    Assert.AreEqual(1, _displayedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.LogicDoorGuid), _displayedMessages));

        ////    Assert.AreEqual(1, _removedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.LogicDoorGuid), _removedMessages));

        ////    _meters[ApplicationMeters.BellyDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.BellyDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenCount].Verify(m => m.Increment(1), Times.Once());
        ////    _meters[ApplicationMeters.LogicDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////}

        ////[TestMethod]
        ////public void HandleDoorOpenLogicDoorPowerOffTest()
        ////{
        ////    _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();

        ////    var theEvent = new OpenEvent(LogicDoorId, true, "Logic Door"); // While powered down
        ////    MockLog(false, theEvent.LogicalId, theEvent.Timestamp, true, true);
        ////    _meters[ApplicationMeters.LogicDoorOpenPowerOffCount].Setup(m => m.Increment(1)).Verifiable();
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.LogicDoorGuid,
        ////        "Logic Door is Open",
        ////        "Logic Door Closed");

        ////    _iio = MoqServiceManager.CreateAndAddService<IIO>(MockBehavior.Strict);
        ////    _iio.Setup(m => m.GetInputs).Returns(35184372088888)
        ////        .Verifiable(); // Mask value to return true from IsDoorOpenOnPowerUp for logic door 

        ////    _accessor.HandleEvent(theEvent);

        ////    Assert.AreEqual(1, _displayedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.LogicDoorGuid), _displayedMessages));

        ////    Assert.AreEqual(1, _removedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.LogicDoorGuid), _removedMessages));

        ////    _meters[ApplicationMeters.BellyDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.BellyDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Once());
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////}

        ////[TestMethod]
        ////public void HandleDoorOpenMainDoorTest()
        ////{
        ////    _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();

        ////    var theEvent = new OpenEvent(MainDoorId, "Main Door");
        ////    MockLog(false, theEvent.LogicalId, theEvent.Timestamp, true, true);
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.MainDoorGuid,
        ////        "Main Door is Open",
        ////        "Main Door Closed");

        ////    _accessor.HandleEvent(theEvent);

        ////    Assert.AreEqual(1, _displayedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.MainDoorGuid), _displayedMessages));

        ////    Assert.AreEqual(1, _removedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.MainDoorGuid), _removedMessages));

        ////    _meters[ApplicationMeters.BellyDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.BellyDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Verify(m => m.Increment(1), Times.Once());
        ////    _meters[ApplicationMeters.MainDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////}

        ////[TestMethod]
        ////public void HandleDoorOpenTopBoxDoorTest()
        ////{
        ////    _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();

        ////    var theEvent = new OpenEvent(TopBoxDoorId, "Top Box Door");
        ////    MockLog(false, theEvent.LogicalId, theEvent.Timestamp, true, true);
        ////    _meters[ApplicationMeters.TopBoxDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.TopBoxDoorGuid,
        ////        "Top Box Door is Open",
        ////        "Top Box Door Closed");

        ////    _accessor.HandleEvent(theEvent);

        ////    Assert.AreEqual(1, _displayedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.TopBoxDoorGuid), _displayedMessages));

        ////    Assert.AreEqual(1, _removedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.TopBoxDoorGuid), _removedMessages));

        ////    _meters[ApplicationMeters.BellyDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.BellyDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenCount].Verify(m => m.Increment(1), Times.Once());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////}

        ////[TestMethod]
        ////public void HandleDoorOpenPizzaBoxDoorTest()
        ////{
        ////    _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();

        ////    var theEvent = new OpenEvent(PizzaBoxDoorId);
        ////    MockLog(false, theEvent.LogicalId, theEvent.Timestamp, true, true);
        ////    _meters[ApplicationMeters.PizzaBoxDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.PizzaBoxDoorGuid,
        ////        Resources.PizzaBoxDoorIsOpen,
        ////        Resources.PizzaBoxDoorClosed);

        ////    _accessor.HandleEvent(theEvent);

        ////    Assert.AreEqual(1, _displayedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.PizzaBoxDoorGuid), _displayedMessages));

        ////    Assert.AreEqual(1, _removedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.PizzaBoxDoorGuid), _removedMessages));

        ////    _meters[ApplicationMeters.BellyDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.BellyDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////}

        ////[TestMethod]
        ////public void ProcessDoorEventManyEventTest()
        ////{
        ////    // Test the multiple open events.
        ////    var cashDoorOpenEvent = new OpenEvent(CashDoorId);
        ////    var logicDoorOpenEvent = new OpenEvent(LogicDoorId);
        ////    var mainDoorOpenEvent = new OpenEvent(MainDoorId);
        ////    var topBoxDoorOpenEvent = new OpenEvent(TopBoxDoorId);

        ////    MockLog(true, cashDoorOpenEvent.LogicalId, cashDoorOpenEvent.Timestamp, true, true);
        ////    MockLog(true, logicDoorOpenEvent.LogicalId, logicDoorOpenEvent.Timestamp, true, true);
        ////    MockLog(true, mainDoorOpenEvent.LogicalId, mainDoorOpenEvent.Timestamp, true, true);
        ////    MockLog(true, topBoxDoorOpenEvent.LogicalId, topBoxDoorOpenEvent.Timestamp, true, true);

        ////    _meters[ApplicationMeters.CashDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();
        ////    _meters[ApplicationMeters.LogicDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();
        ////    _meters[ApplicationMeters.TopBoxDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();

        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.CashDoorGuid,
        ////        "Cash Door is Open,
        ////        "Cash Door Closed");
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.LogicDoorGuid,
        ////        "Logic Door is Open",
        ////        "Logic Door Closed");
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.MainDoorGuid,
        ////        "Main Door is Open",
        ////        "Main Door Closed");
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.TopBoxDoorGuid,
        ////        "Top Box Door is Open",
        ////        "Top Box Door Closed");

        ////    _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();

        ////    _accessor.HandleEvent(cashDoorOpenEvent);
        ////    _accessor.HandleEvent(logicDoorOpenEvent);
        ////    _accessor.HandleEvent(mainDoorOpenEvent);
        ////    _accessor.HandleEvent(topBoxDoorOpenEvent);

        ////    _messageDisplay.Verify();
        ////    Assert.AreEqual(4, _displayedMessages.Count);

        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.CashDoorGuid), _displayedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.LogicDoorGuid), _displayedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.MainDoorGuid), _displayedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.TopBoxDoorGuid), _displayedMessages));

        ////    Assert.AreEqual(4, _removedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.CashDoorGuid), _removedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.LogicDoorGuid), _removedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.MainDoorGuid), _removedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.TopBoxDoorGuid), _removedMessages));

        ////    _meters[ApplicationMeters.CashDoorOpenCount].Verify(m => m.Increment(1), Times.Exactly(1));
        ////    _meters[ApplicationMeters.LogicDoorOpenCount].Verify(m => m.Increment(1), Times.Exactly(1));
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Verify(m => m.Increment(1), Times.Exactly(1));
        ////    _meters[ApplicationMeters.TopBoxDoorOpenCount].Verify(m => m.Increment(1), Times.Exactly(1));

        ////    // Test the multiple closed events.
        ////    var logicDoorClosedEvent = new ClosedEvent(LogicDoorId);
        ////    var mainDoorClosedEvent = new ClosedEvent(MainDoorId);

        ////    MockDisableManagerAndMessageDisplay(
        ////        false,
        ////        _accessor.LogicDoorGuid,
        ////        "Logic Door is Open",
        ////        "Logic Door Closed");
        ////    MockDisableManagerAndMessageDisplay(
        ////        false,
        ////        _accessor.MainDoorGuid,
        ////        "Main Door is Open",
        ////        "Main Door Closed");

        ////    MockLog(true, logicDoorClosedEvent.LogicalId, logicDoorOpenEvent.Timestamp, true, true);
        ////    MockLog(true, mainDoorClosedEvent.LogicalId, mainDoorOpenEvent.Timestamp, true, true);
        ////    _persistentStorage.Setup(m => m.VerifyIntegrity(true)).Returns(true).Verifiable();
        ////    _iio.Setup(m => m.ResetPhysicalDoorWasOpened(It.IsAny<int>()));

        ////    _accessor.HandleEvent(new ClosedEvent(LogicDoorId));
        ////    _accessor.HandleEvent(new ClosedEvent(MainDoorId));

        ////    Assert.AreEqual(6, _displayedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.CashDoorGuid), _displayedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.LogicDoorGuid), _displayedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.MainDoorGuid), _displayedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.TopBoxDoorGuid), _displayedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.LogicDoorGuid), _displayedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.MainDoorGuid), _displayedMessages));

        ////    Assert.AreEqual(6, _removedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.CashDoorGuid), _removedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.LogicDoorGuid), _removedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.MainDoorGuid), _removedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.TopBoxDoorGuid), _removedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.LogicDoorGuid), _removedMessages));
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.MainDoorGuid), _removedMessages));

        ////    // Meters are not updated for the closed events.
        ////    _meters[ApplicationMeters.CashDoorOpenCount].Verify(m => m.Increment(1), Times.Exactly(1));
        ////    _meters[ApplicationMeters.LogicDoorOpenCount].Verify(m => m.Increment(1), Times.Exactly(1));
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Verify(m => m.Increment(1), Times.Exactly(1));
        ////    _meters[ApplicationMeters.TopBoxDoorOpenCount].Verify(m => m.Increment(1), Times.Exactly(1));
        ////}

        ////[TestMethod]
        ////public void HandleDoorOpenBellyDoorTest()
        ////{
        ////    _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();

        ////    var theEvent = new OpenEvent(BellyDoorId);
        ////    MockLog(false, theEvent.LogicalId, theEvent.Timestamp, true, true);
        ////    _meters[ApplicationMeters.BellyDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.BellyDoorGuid,
        ////        "Belly Door is Open",
        ////        "Belly Door Closed");

        ////    _accessor.HandleEvent(theEvent);

        ////    Assert.IsTrue(_accessor._logicalDoorsLoaded);

        ////    Assert.AreEqual(1, _displayedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.BellyDoorGuid), _displayedMessages));

        ////    Assert.AreEqual(1, _removedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.BellyDoorGuid), _removedMessages));

        ////    _meters[ApplicationMeters.BellyDoorOpenCount].Verify(m => m.Increment(1), Times.Once());
        ////    _meters[ApplicationMeters.BellyDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////}

        ////[TestMethod]
        ////public void HandleDoorOpenCashDoorTest()
        ////{
        ////    _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();

        ////    var theEvent = new OpenEvent(CashDoorId);
        ////    MockLog(false, theEvent.LogicalId, theEvent.Timestamp, true, true);
        ////    _meters[ApplicationMeters.CashDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.CashDoorGuid,
        ////        "Cash Door is Open,
        ////        "Cash Door Closed");

        ////    _accessor.HandleEvent(theEvent);

        ////    Assert.AreEqual(1, _displayedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.CashDoorGuid), _displayedMessages));

        ////    Assert.AreEqual(1, _removedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.CashDoorGuid), _removedMessages));

        ////    _meters[ApplicationMeters.BellyDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.BellyDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenCount].Verify(m => m.Increment(1), Times.Once());
        ////    _meters[ApplicationMeters.CashDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////}

        ////[TestMethod]
        ////public void HandleDoorOpenLogicDoorTest()
        ////{
        ////    _eventBus.Setup(m => m.Publish(It.IsAny<DoorHardTiltEvent>())).Verifiable();
        ////    _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();

        ////    var theEvent = new OpenEvent(LogicDoorId);
        ////    MockLog(false, theEvent.LogicalId, theEvent.Timestamp, true, true);
        ////    _meters[ApplicationMeters.LogicDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.LogicDoorGuid,
        ////        "Logic Door is Open",
        ////        "Logic Door Closed");

        ////    _accessor.HandleEvent(theEvent);

        ////    Assert.AreEqual(1, _displayedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.LogicDoorGuid), _displayedMessages));

        ////    Assert.AreEqual(1, _removedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.LogicDoorGuid), _removedMessages));

        ////    _meters[ApplicationMeters.BellyDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.BellyDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenCount].Verify(m => m.Increment(1), Times.Once());
        ////    _meters[ApplicationMeters.LogicDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////}

        ////[TestMethod]
        ////public void HandleDoorOpenLogicDoorPowerOffTest()
        ////{
        ////    _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();

        ////    var theEvent = new OpenEvent(LogicDoorId, true); // While powered down
        ////    MockLog(false, theEvent.LogicalId, theEvent.Timestamp, true, true);
        ////    _meters[ApplicationMeters.LogicDoorOpenPowerOffCount].Setup(m => m.Increment(1)).Verifiable();
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.LogicDoorGuid,
        ////        "Logic Door is Open",
        ////        "Logic Door Closed");

        ////    _iio = MoqServiceManager.CreateAndAddService<IIO>(MockBehavior.Strict);
        ////    _iio.Setup(m => m.GetInputs).Returns(35184372088888)
        ////        .Verifiable(); // Mask value to return true from IsDoorOpenOnPowerUp for logic door 

        ////    _accessor.HandleEvent(theEvent);

        ////    Assert.AreEqual(1, _displayedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.LogicDoorGuid), _displayedMessages));

        ////    Assert.AreEqual(1, _removedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.LogicDoorGuid), _removedMessages));

        ////    _meters[ApplicationMeters.BellyDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.BellyDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Once());
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////}

        ////[TestMethod]
        ////public void HandleDoorOpenMainDoorTest()
        ////{
        ////    _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();

        ////    var theEvent = new OpenEvent(MainDoorId);
        ////    MockLog(false, theEvent.LogicalId, theEvent.Timestamp, true, true);
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.MainDoorGuid,
        ////        "Main Door is Open",
        ////        "Main Door Closed");

        ////    _accessor.HandleEvent(theEvent);

        ////    Assert.AreEqual(1, _displayedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.MainDoorGuid), _displayedMessages));

        ////    Assert.AreEqual(1, _removedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.MainDoorGuid), _removedMessages));

        ////    _meters[ApplicationMeters.BellyDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.BellyDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Verify(m => m.Increment(1), Times.Once());
        ////    _meters[ApplicationMeters.MainDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////}

        ////[TestMethod]
        ////public void HandleDoorOpenTopBoxDoorTest()
        ////{
        ////    _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();

        ////    var theEvent = new OpenEvent(TopBoxDoorId);
        ////    MockLog(false, theEvent.LogicalId, theEvent.Timestamp, true, true);
        ////    _meters[ApplicationMeters.TopBoxDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.TopBoxDoorGuid,
        ////        "Top Box Door is Open",
        ////        "Top Box Door Closed");

        ////    _accessor.HandleEvent(theEvent);

        ////    Assert.AreEqual(1, _displayedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.TopBoxDoorGuid), _displayedMessages));

        ////    Assert.AreEqual(1, _removedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.TopBoxDoorGuid), _removedMessages));

        ////    _meters[ApplicationMeters.BellyDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.BellyDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.CashDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.LogicDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.MainDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenCount].Verify(m => m.Increment(1), Times.Once());
        ////    _meters[ApplicationMeters.TopBoxDoorOpenPowerOffCount].Verify(m => m.Increment(1), Times.Never());
        ////}

        [TestMethod]
        public void HandleDoorOpenUnknownDoorTest()
        {
            _edgeLightingStateManager.Setup(x => x.SetState(EdgeLightState.DoorOpen)).Returns(null as IEdgeLightToken);
            var theEvent = new OpenEvent(999, "Unknown Door");
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.DoorAlarmEnabledKey, It.IsAny<bool>())).Returns(true);

            _accessor.HandleEvent(theEvent);
        }

        [TestMethod]
        public void HandleDoorClosedBeforeDoorsLoadedBellyDoorTest()
        {
            var theEvent = new ClosedEvent(BellyDoorId, "Belly Door");
            MockDisableManagerAndMessageDisplay(
                false,
                ApplicationConstants.BellyDoorGuid,
                "Belly Door is Open",
                "Belly Door Closed");
            MockLog(false, theEvent.LogicalId, theEvent.Timestamp, false, true);
            _persistentStorage.Setup(m => m.VerifyIntegrity(true)).Returns(true).Verifiable();
            _iio.Setup(m => m.ResetPhysicalDoorWasOpened(It.IsAny<int>()));

            _accessor._logicalDoorsLoaded = false;

            _accessor.HandleEvent(theEvent);

            Assert.AreEqual(1, _accessor._queuedStartUpEvents.Count);
            Assert.AreEqual(0, _displayedMessages.Count);
            Assert.AreEqual(0, _removedMessages.Count);
        }

        [TestMethod]
        public void HandleDoorClosedCashDoorTest()
        {
            var theEvent = new ClosedEvent(CashDoorId, "Stacker Door");
            MockDisableManagerAndMessageDisplay(
                false,
                ApplicationConstants.CashDoorGuid,
                "Stacker Door is Open",
                "Stacker Door Closed");
            MockLog(false, theEvent.LogicalId, theEvent.Timestamp, false, true);
            _persistentStorage.Setup(m => m.VerifyIntegrity(true)).Returns(true).Verifiable();
            _iio.Setup(m => m.ResetPhysicalDoorWasOpened(It.IsAny<int>()));

            _accessor.HandleEvent(theEvent);

            Assert.AreEqual(0, _displayedMessages.Count);
            Assert.AreEqual(0, _removedMessages.Count);
        }

        [TestMethod]
        public void HandleDoorClosedLogicDoorTest()
        {
            var theEvent = new ClosedEvent(LogicDoorId, "Logic Door");
            MockDisableManagerAndMessageDisplay(
                false,
                ApplicationConstants.LogicDoorGuid,
                "Logic Door is Open",
                "Logic Door Closed");
            MockLog(false, theEvent.LogicalId, theEvent.Timestamp, false, true);
            _persistentStorage.Setup(m => m.VerifyIntegrity(true)).Returns(true).Verifiable();
            _iio.Setup(m => m.ResetPhysicalDoorWasOpened(It.IsAny<int>()));

            _accessor.HandleEvent(theEvent);

            Assert.AreEqual(0, _displayedMessages.Count);
            Assert.AreEqual(0, _removedMessages.Count);
        }

        [TestMethod]
        public void HandleDoorClosedMainDoorTest()
        {
            var theEvent = new ClosedEvent(MainDoorId, "Main Door");
            MockDisableManagerAndMessageDisplay(
                false,
                ApplicationConstants.MainDoorGuid,
                "Main Door is Open",
                "Main Door Closed");
            MockLog(false, theEvent.LogicalId, theEvent.Timestamp, false, true);
            _persistentStorage.Setup(m => m.VerifyIntegrity(true)).Returns(true).Verifiable();
            _iio.Setup(m => m.ResetPhysicalDoorWasOpened(It.IsAny<int>()));

            _accessor.HandleEvent(theEvent);

            Assert.AreEqual(0, _displayedMessages.Count);
            Assert.AreEqual(0, _removedMessages.Count);
        }

        [TestMethod]
        public void HandleDoorClosedTopBoxDoorTest()
        {
            var theEvent = new ClosedEvent(TopBoxDoorId, "Top Box Door");
            MockDisableManagerAndMessageDisplay(
                false,
                ApplicationConstants.TopBoxDoorGuid,
                "Top Box Door is Open",
                "Top Box Door Closed");
            MockLog(false, theEvent.LogicalId, theEvent.Timestamp, false, true);
            _persistentStorage.Setup(m => m.VerifyIntegrity(true)).Returns(true).Verifiable();
            _iio.Setup(m => m.ResetPhysicalDoorWasOpened(It.IsAny<int>()));

            _accessor.HandleEvent(theEvent);

            Assert.AreEqual(0, _displayedMessages.Count);
            Assert.AreEqual(0, _removedMessages.Count);
        }

        [TestMethod]
        public void HandleDoorClosedMechanicalMeterDoorTest()
        {
            var theEvent = new ClosedEvent(MechanicalMeterDoorId, "MechanicalMeter Door");
            MockDisableManagerAndMessageDisplay(
                false,
                ApplicationConstants.MechanicalMeterDoorGuid,
                "Mechanical Meter Door is Open",
                "Mechanical Meter Door Closed");
            MockLog(false, theEvent.LogicalId, theEvent.Timestamp, false, true);
            _persistentStorage.Setup(m => m.VerifyIntegrity(true)).Returns(true).Verifiable();
            _iio.Setup(m => m.ResetPhysicalDoorWasOpened(It.IsAny<int>()));

            _accessor.HandleEvent(theEvent);

            Assert.AreEqual(0, _displayedMessages.Count);
            Assert.AreEqual(0, _removedMessages.Count);
        }

        [TestMethod]
        public void HandleDoorClosedMainOpticDoorTest()
        {
            var theEvent = new ClosedEvent(MainOpticDoorId, "Main Optic Door");
            MockDisableManagerAndMessageDisplay(
                false,
                ApplicationConstants.MainOpticDoorGuid,
                "Main Optic Door is Open",
                "Main Optic Door Closed");
            MockLog(false, theEvent.LogicalId, theEvent.Timestamp, false, true);
            _persistentStorage.Setup(m => m.VerifyIntegrity(true)).Returns(true).Verifiable();
            _iio.Setup(m => m.ResetPhysicalDoorWasOpened(It.IsAny<int>()));

            _accessor.HandleEvent(theEvent);

            Assert.AreEqual(0, _displayedMessages.Count);
            Assert.AreEqual(0, _removedMessages.Count);
        }

        [TestMethod]
        public void HandleDoorClosedTopBoxOpticDoorTest()
        {
            var theEvent = new ClosedEvent(TopBoxOpticDoorId, "Top Box Optic Door");
            MockDisableManagerAndMessageDisplay(
                false,
                ApplicationConstants.TopBoxOpticDoorGuid,
                "Top Box Optic Door is Open",
                "Top Box Optic Door Closed");
            MockLog(false, theEvent.LogicalId, theEvent.Timestamp, false, true);
            _persistentStorage.Setup(m => m.VerifyIntegrity(true)).Returns(true).Verifiable();
            _iio.Setup(m => m.ResetPhysicalDoorWasOpened(It.IsAny<int>()));

            _accessor.HandleEvent(theEvent);

            Assert.AreEqual(0, _displayedMessages.Count);
            Assert.AreEqual(0, _removedMessages.Count);
        }

        [TestMethod]
        public void HandleDoorClosedUniversalInterfaceBoxDoorTest()
        {
            var theEvent = new ClosedEvent(UniversalInterfaceBoxDoorId, "Universal Interface Box Door");
            MockDisableManagerAndMessageDisplay(
                false,
                ApplicationConstants.UniversalInterfaceBoxDoorGuid,
                "Universal Interface Box Door is Open",
                "Universal Interface Box Door Closed");
            MockLog(false, theEvent.LogicalId, theEvent.Timestamp, false, true);
            _persistentStorage.Setup(m => m.VerifyIntegrity(true)).Returns(true).Verifiable();
            _iio.Setup(m => m.ResetPhysicalDoorWasOpened(It.IsAny<int>()));

            _accessor.HandleEvent(theEvent);

            Assert.AreEqual(0, _displayedMessages.Count);
            Assert.AreEqual(0, _removedMessages.Count);
        }

        [TestMethod]
        public void HandleDoorClosedUnknownDoorTest()
        {
            var theEvent = new ClosedEvent(999, "Unknown Door");

            _accessor.HandleEvent(theEvent);
        }

        [TestMethod]
        public void IncrementBellyDoorMeterTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();
            _meters[ApplicationMeters.BellyDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();

            _accessor.IncrementDoorMeter(BellyDoorId, false, "Belly Door");

            _meters[ApplicationMeters.BellyDoorOpenCount].Verify(m => m.Increment(1), Times.Once());
        }

        [TestMethod]
        public void DisableCashDoorTest()
        {
            var disableGuid = new Guid(ApplicationConstants.CashDoorGuid.ToString());
            const SystemDisablePriority disablePriority = SystemDisablePriority.Immediate;
            var doorOpenMessage = "Stacker Door is Open";
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.CashDoorGuid,
                "Stacker Door is Open",
                "Stacker Door Closed");

            _accessor.OpenDoor(CashDoorId, null);

            Assert.AreEqual(1, _displayedMessages.Count);

            Assert.IsTrue(IsMessageInList(doorOpenMessage, _displayedMessages));

            _disableManager.Verify(m => m.Disable(disableGuid, disablePriority, It.Is<Func<string>>(x => x.Invoke() == doorOpenMessage), null), Times.Once());
        }

        [TestMethod]
        public void IncrementCashDoorMeterTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();
            _meters[ApplicationMeters.CashDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();

            _accessor.IncrementDoorMeter(CashDoorId, false, "Stacker Door");

            _meters[ApplicationMeters.CashDoorOpenCount].Verify(m => m.Increment(1), Times.Once());
        }

        [TestMethod]
        public void DisableLogicDoorTest()
        {
            var disableGuid = new Guid(ApplicationConstants.LogicDoorGuid.ToString());
            const SystemDisablePriority disablePriority = SystemDisablePriority.Immediate;
            var doorOpenMessage = "Logic Door is Open";
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.LogicDoorGuid,
                "Logic Door is Open",
                "Logic Door Closed");

            _accessor.OpenDoor(LogicDoorId, null);

            Assert.AreEqual(1, _displayedMessages.Count);

            Assert.IsTrue(IsMessageInList(doorOpenMessage, _displayedMessages));

            _disableManager.Verify(m => m.Disable(disableGuid, disablePriority, It.Is<Func<string>>(x => x.Invoke() == doorOpenMessage), null), Times.Once());
        }

        [TestMethod]
        public void IncrementLogicDoorMeterTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();
            _meters[ApplicationMeters.LogicDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();

            _accessor.IncrementDoorMeter(LogicDoorId, false, "Logic Door");

            _meters[ApplicationMeters.LogicDoorOpenCount].Verify(m => m.Increment(1), Times.Once());
        }

        [TestMethod]
        public void DisableMainDoorTest()
        {
            var disableGuid = new Guid(ApplicationConstants.MainDoorGuid.ToString());
            const SystemDisablePriority disablePriority = SystemDisablePriority.Immediate;
            var doorOpenMessage = "Main Door is Open";
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.MainDoorGuid,
                "Main Door is Open",
                "Main Door Closed");

            _accessor.OpenDoor(MainDoorId, null);

            Assert.AreEqual(1, _displayedMessages.Count);

            Assert.IsTrue(IsMessageInList(doorOpenMessage, _displayedMessages));
            _disableManager.Verify(m => m.Disable(disableGuid, disablePriority, It.Is<Func<string>>(x => x.Invoke() == doorOpenMessage), null), Times.Once());
        }

        [TestMethod]
        public void IncrementMainDoorMeterTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();
            _meters[ApplicationMeters.MainDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();

            _accessor.IncrementDoorMeter(MainDoorId, false, "Main Door");

            _meters[ApplicationMeters.MainDoorOpenCount].Verify(m => m.Increment(1), Times.Once());
        }

        [TestMethod]
        public void DisableTopBoxDoorTest()
        {
            var disableGuid = new Guid(ApplicationConstants.TopBoxDoorGuid.ToString());
            const SystemDisablePriority disablePriority = SystemDisablePriority.Immediate;
            var doorOpenMessage = "Top Box Door is Open";
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.TopBoxDoorGuid,
                "Top Box Door is Open",
                "Top Box Door Closed");

            _accessor.OpenDoor(TopBoxDoorId, null);

            Assert.AreEqual(1, _displayedMessages.Count);

            Assert.IsTrue(IsMessageInList(doorOpenMessage, _displayedMessages));
            _disableManager.Verify(m => m.Disable(disableGuid, disablePriority, It.Is<Func<string>>(x => x.Invoke() == doorOpenMessage), null), Times.Once());
        }

        [TestMethod]
        public void IncrementTopBoxDoorMeterTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();
            _meters[ApplicationMeters.TopBoxDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();

            _accessor.IncrementDoorMeter(TopBoxDoorId, false, "Top Box Door");

            _meters[ApplicationMeters.TopBoxDoorOpenCount].Verify(m => m.Increment(1), Times.Once());
        }

        [TestMethod]
        public void DisableUniversalInterfaceBoxDoorTest()
        {
            var disableGuid = new Guid(ApplicationConstants.UniversalInterfaceBoxDoorGuid.ToString());
            const SystemDisablePriority disablePriority = SystemDisablePriority.Immediate;
            var doorOpenMessage = "Universal Interface Box Door is Open";
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.UniversalInterfaceBoxDoorGuid,
                "Universal Interface Box Door is Open",
                "Universal Interface Box Door Closed");

            _accessor.OpenDoor(UniversalInterfaceBoxDoorId, null);

            Assert.AreEqual(1, _displayedMessages.Count);

            Assert.IsTrue(IsMessageInList(doorOpenMessage, _displayedMessages));
            _disableManager.Verify(m => m.Disable(disableGuid, disablePriority, It.Is<Func<string>>(x => x.Invoke() == doorOpenMessage), null), Times.Once());
        }

        [TestMethod]
        public void IncrementUniversalInterfaceBoxDoorMeterTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();
            _meters[ApplicationMeters.TopBoxDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();

            _accessor.IncrementDoorMeter(UniversalInterfaceBoxDoorId, false, "Universal Interface Box Door");

            _meters[ApplicationMeters.TopBoxDoorOpenCount].Verify(m => m.Increment(1), Times.Once());
        }

        [TestMethod]
        public void DisableMechanicalMeterDoorTest()
        {
            var disableGuid = new Guid(ApplicationConstants.MechanicalMeterDoorGuid.ToString());
            const SystemDisablePriority disablePriority = SystemDisablePriority.Immediate;
            var doorOpenMessage = "Mechanical Meter Door is Open";
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.MechanicalMeterDoorGuid,
                "Mechanical Meter Door is Open",
                "Mechanical Meter Door Closed");

            _accessor.OpenDoor(MechanicalMeterDoorId, null);

            Assert.AreEqual(1, _displayedMessages.Count);

            Assert.IsTrue(IsMessageInList(doorOpenMessage, _displayedMessages));
            _disableManager.Verify(m => m.Disable(disableGuid, disablePriority, It.Is<Func<string>>(x => x.Invoke() == doorOpenMessage), null), Times.Once());
        }

        [TestMethod]
        public void IncrementMechanicalMeterDoorMeterTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();
            _meters[ApplicationMeters.MechanicalMeterDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();

            _accessor.IncrementDoorMeter(MechanicalMeterDoorId, false, "Mechanical Meter Door");

            _meters[ApplicationMeters.MechanicalMeterDoorOpenCount].Verify(m => m.Increment(1), Times.Once());
        }

        [TestMethod]
        public void DisableMainOpticDoorTest()
        {
            var disableGuid = new Guid(ApplicationConstants.MainOpticDoorGuid.ToString());
            const SystemDisablePriority disablePriority = SystemDisablePriority.Immediate;
            var doorOpenMessage = "Main Door is Open";
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.MainOpticDoorGuid,
                "Main Door is Open",
                "Main Optic Door Closed");

            _accessor.OpenDoor(MainOpticDoorId, null);

            Assert.AreEqual(1, _displayedMessages.Count);

            Assert.IsTrue(IsMessageInList(doorOpenMessage, _displayedMessages));
            _disableManager.Verify(m => m.Disable(disableGuid, disablePriority, It.Is<Func<string>>(x => x.Invoke() == doorOpenMessage), null), Times.Once());
        }

        [TestMethod]
        public void IncrementMainOpticDoorMeterTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();
            _meters[ApplicationMeters.MainOpticDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();

            _accessor.IncrementDoorMeter(MainOpticDoorId, false, "Main Optic Door");

            _meters[ApplicationMeters.MainOpticDoorOpenCount].Verify(m => m.Increment(1), Times.Once());
        }

        [TestMethod]
        public void DisableTopBoxOpticDoorTest()
        {
            var disableGuid = new Guid(ApplicationConstants.TopBoxOpticDoorGuid.ToString());
            const SystemDisablePriority disablePriority = SystemDisablePriority.Immediate;
            var doorOpenMessage = "Top Box Door is Open";
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.TopBoxOpticDoorGuid,
                "Top Box Door is Open",
                "Top Box Optic Door Closed");

            _accessor.OpenDoor(TopBoxOpticDoorId, null);

            Assert.AreEqual(1, _displayedMessages.Count);

            Assert.IsTrue(IsMessageInList(doorOpenMessage, _displayedMessages));
            _disableManager.Verify(m => m.Disable(disableGuid, disablePriority, It.Is<Func<string>>(x => x.Invoke() == doorOpenMessage), null), Times.Once());
        }

        [TestMethod]
        public void IncrementTopBoxOpticDoorMeterTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();
            _meters[ApplicationMeters.TopBoxOpticDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();

            _accessor.IncrementDoorMeter(TopBoxOpticDoorId, false, "Top Box Optic Door");

            _meters[ApplicationMeters.TopBoxOpticDoorOpenCount].Verify(m => m.Increment(1), Times.Once());
        }

        [TestMethod]
        public void IncrementDoorMeterWhenMeterNotProvidedTest()
        {
            string name = ApplicationMeters.LogicDoorOpenCount;
            _meterManager.Setup(m => m.IsMeterProvided(name)).Returns(false).Verifiable();

            _accessor.IncrementDoorMeter(LogicDoorId, false, "Logic Door");

            _meterManager.Verify();
        }

        [TestMethod]
        public void SyncLifetimeMeterWhenMeterNotProvidedTest()
        {
            string name = ApplicationMeters.LogicDoorOpenCount;
            _meterManager.Setup(m => m.IsMeterProvided(name)).Returns(false).Verifiable();

            _accessor.SyncLifetimeMeter(LogicDoorId, false);

            _meterManager.Verify();
        }

        [TestMethod]
        public void DisableManyDoorTest()
        {
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.LogicDoorGuid,
                "Logic Door is Open",
                "Logic Door Closed");
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.CashDoorGuid,
                "Stacker Door is Open",
                "Stacker Door Closed");
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.MainDoorGuid,
                "Main Door is Open",
                "Main Door Closed");
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.BellyDoorGuid,
                "Belly Door is Open",
                "Belly Door Closed");
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.TopBoxDoorGuid,
                "Top Box Door is Open",
                "Top Box Door Closed");
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.UniversalInterfaceBoxDoorGuid,
                "Universal Interface Box Door is Open",
                "Universal Interface Box Door Closed");

            _accessor.OpenDoor(LogicDoorId, null);
            _accessor.OpenDoor(CashDoorId, null);
            _accessor.OpenDoor(MainDoorId, null);
            _accessor.OpenDoor(BellyDoorId, null);
            _accessor.OpenDoor(TopBoxDoorId, null);
            _accessor.OpenDoor(UniversalInterfaceBoxDoorId, null);

            Assert.AreEqual(6, _displayedMessages.Count);
            Assert.IsTrue(IsMessageInList("Logic Door is Open", _displayedMessages));
            Assert.IsTrue(IsMessageInList("Stacker Door is Open", _displayedMessages));
            Assert.IsTrue(IsMessageInList("Main Door is Open", _displayedMessages));
            Assert.IsTrue(IsMessageInList("Belly Door is Open", _displayedMessages));
            Assert.IsTrue(IsMessageInList("Top Box Door is Open", _displayedMessages));
            Assert.IsTrue(IsMessageInList("Universal Interface Box Door is Open", _displayedMessages));
            var logicDoorGuid = new Guid(ApplicationConstants.LogicDoorGuid.ToString()).ToByteArray();
            var cashDoorGuid = new Guid(ApplicationConstants.CashDoorGuid.ToString()).ToByteArray();
            var mainDoorGuid = new Guid(ApplicationConstants.MainDoorGuid.ToString()).ToByteArray();
            var bellyDoorGuid = new Guid(ApplicationConstants.BellyDoorGuid.ToString()).ToByteArray();
            var topBoxDoorGuid = new Guid(ApplicationConstants.TopBoxDoorGuid.ToString()).ToByteArray();
            var universalInterfaceBoxDoorGuid = new Guid(ApplicationConstants.UniversalInterfaceBoxDoorGuid.ToString()).ToByteArray();

            _disableManager.Verify(
                m => m.Disable(new Guid(logicDoorGuid), SystemDisablePriority.Immediate, It.Is<Func<string>>(x => x.Invoke() == "Logic Door is Open"), null),
                Times.Once());
            _disableManager.Verify(
                m => m.Disable(new Guid(cashDoorGuid), SystemDisablePriority.Immediate, It.Is<Func<string>>(x => x.Invoke() == "Stacker Door is Open"), null),
                Times.Once());
            _disableManager.Verify(
                m => m.Disable(new Guid(mainDoorGuid), SystemDisablePriority.Immediate, It.Is<Func<string>>(x => x.Invoke() == "Main Door is Open"), null),
                Times.Once());
            _disableManager.Verify(
                m => m.Disable(new Guid(bellyDoorGuid), SystemDisablePriority.Immediate, It.Is<Func<string>>(x => x.Invoke() == "Belly Door is Open"), null),
                Times.Once());
            _disableManager.Verify(
                m => m.Disable(new Guid(topBoxDoorGuid), SystemDisablePriority.Immediate, It.Is<Func<string>>(x => x.Invoke() == "Top Box Door is Open"), null),
                Times.Once());
            _disableManager.Verify(
                m => m.Disable(new Guid(universalInterfaceBoxDoorGuid), SystemDisablePriority.Immediate, It.Is<Func<string>>(x => x.Invoke() == "Universal Interface Box Door is Open"), null),
                Times.Once());
        }

        [TestMethod]
        public void DisableBellyDoorTwiceTest()
        {
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.BellyDoorGuid,
                "Belly Door is Open",
                "Belly Door is Open");

            _accessor.OpenDoor(BellyDoorId, null);
            _accessor.OpenDoor(BellyDoorId, null);

            Assert.AreEqual(2, _displayedMessages.Count);
            Assert.AreEqual("Belly Door is Open", _displayedMessages[0]);
            Assert.AreEqual("Belly Door is Open", _displayedMessages[1]);
        }

        [TestMethod]
        public void EnableBellyDoorTest()
        {
            MockDisableManagerAndMessageDisplay(
                false,
                ApplicationConstants.BellyDoorGuid,
                "Belly Door is Open",
                "Belly Door Closed");
            _accessor.CloseDoor(BellyDoorId);
            Assert.AreEqual(0, _displayedMessages.Count);
        }

        [TestMethod]
        public void EnableCashDoorTest()
        {
            MockDisableManagerAndMessageDisplay(
                false,
                ApplicationConstants.CashDoorGuid,
                "Stacker Door is Open",
                "Stacker Door Closed");
            _accessor.CloseDoor(CashDoorId);
            Assert.AreEqual(0, _displayedMessages.Count);
        }

        [TestMethod]
        public void EnableLogicDoorTest()
        {
            MockDisableManagerAndMessageDisplay(
                false,
                ApplicationConstants.LogicDoorGuid,
                "Logic Door is Open",
                "Logic Door Closed");
            _accessor.CloseDoor(LogicDoorId);
            Assert.AreEqual(0, _displayedMessages.Count);
        }

        [TestMethod]
        public void EnableMainDoorTest()
        {
            MockDisableManagerAndMessageDisplay(
                false,
                ApplicationConstants.MainDoorGuid,
                "Main Door is Open",
                "Main Door Closed");
            _accessor.CloseDoor(MainDoorId);
            Assert.AreEqual(0, _displayedMessages.Count);
        }

        [TestMethod]
        public void EnableTopBoxDoorTest()
        {
            MockDisableManagerAndMessageDisplay(
                false,
                ApplicationConstants.TopBoxDoorGuid,
                "Top Box Door is Open",
                "Top Box Door Closed");
            _accessor.CloseDoor(TopBoxDoorId);
            Assert.AreEqual(0, _displayedMessages.Count);
        }

        [TestMethod]
        public void EnableUniversalInterfaceBoxDoorTest()
        {
            MockDisableManagerAndMessageDisplay(
                false,
                ApplicationConstants.UniversalInterfaceBoxDoorGuid,
                "Universal Interface Box Door is Open",
                "Universal Interface Box Door Closed");
            _accessor.CloseDoor(UniversalInterfaceBoxDoorId);
            Assert.AreEqual(0, _displayedMessages.Count);
        }

        /// <summary>
        ///     A test for Enable.  The logic, main, belly, and top box doors are disabled.  The Belly and
        ///     logic doors will be enabled.
        /// </summary>
        [TestMethod]
        public void EnableManyDoorTest()
        {
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.LogicDoorGuid,
                "Logic Door is Open",
                "Logic Door Closed");
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.CashDoorGuid,
                "Stacker Door is Open",
                "Stacker Door Closed");
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.MainDoorGuid,
                "Main Door is Open",
                "Main Door Closed");
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.BellyDoorGuid,
                "Belly Door is Open",
                "Belly Door Closed");
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.TopBoxDoorGuid,
                "Top Box Door is Open",
                "Top Box Door Closed");
            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.UniversalInterfaceBoxDoorGuid,
                "Universal Interface Box Door is Open",
                "Universal Interface Box Door Closed");

            _accessor.OpenDoor(LogicDoorId, null);
            _accessor.OpenDoor(CashDoorId, null);
            _accessor.OpenDoor(MainDoorId, null);
            _accessor.OpenDoor(BellyDoorId, null);
            _accessor.OpenDoor(TopBoxDoorId, null);
            _accessor.OpenDoor(UniversalInterfaceBoxDoorId, null);

            MockDisableManagerAndMessageDisplay(
                false,
                ApplicationConstants.BellyDoorGuid,
                "Belly Door is Open",
                "Belly Door Closed");
            MockDisableManagerAndMessageDisplay(
                false,
                ApplicationConstants.CashDoorGuid,
                "Stacker Door is Open",
                "Stacker Door Closed");
            MockDisableManagerAndMessageDisplay(
                false,
                ApplicationConstants.LogicDoorGuid,
                "Logic Door is Open",
                "Logic Door Closed");

            _accessor.CloseDoor(BellyDoorId);
            _accessor.CloseDoor(CashDoorId);
            _accessor.CloseDoor(LogicDoorId);

            Assert.AreEqual(9, _displayedMessages.Count);
            Assert.IsTrue(IsMessageInList("Logic Door is Open", _displayedMessages));
            Assert.IsTrue(IsMessageInList("Stacker Door is Open", _displayedMessages));
            Assert.IsTrue(IsMessageInList("Main Door is Open", _displayedMessages));
            Assert.IsTrue(IsMessageInList("Belly Door is Open", _displayedMessages));
            Assert.IsTrue(IsMessageInList("Top Box Door is Open", _displayedMessages));
            Assert.IsTrue(IsMessageInList("Universal Interface Box Door is Open", _displayedMessages));

            Assert.IsTrue(IsMessageInList("Belly Door Closed", _displayedMessages));
            Assert.IsTrue(IsMessageInList("Stacker Door Closed", _displayedMessages));
            Assert.IsTrue(IsMessageInList("Logic Door Closed", _displayedMessages));
        }

        [TestMethod]
        public void EnableBellyDoorTwiceTest()
        {
            MockDisableManagerAndMessageDisplay(
                false,
                ApplicationConstants.BellyDoorGuid,
                "Belly Door is Open",
                "Belly Door Closed");
            _accessor.CloseDoor(BellyDoorId);
            Assert.AreEqual(0, _displayedMessages.Count);

            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.BellyDoorGuid,
                "Belly Door is Open",
                "Belly Door Closed");

            _accessor.OpenDoor(BellyDoorId, null);

            _accessor.CloseDoor(BellyDoorId);
            _accessor.CloseDoor(BellyDoorId);

            Assert.AreEqual(2, _displayedMessages.Count);
            Assert.IsTrue(IsMessageInList("Belly Door is Open", _displayedMessages));
            Assert.IsTrue(IsMessageInList("Belly Door Closed", _displayedMessages));
        }

        [TestMethod]
        public void CheckForOpenDoorsWhenAllClosedTest()
        {
            _accessor.CheckForPreviouslyOpenDoors();

            Assert.AreEqual(0, _displayedMessages.Count);
        }

        ////[TestMethod]
        ////public void CheckForPreviouslyOpenDoorsTestWithQueuedEventsTest()
        ////{
        ////    var doors =
        ////        new Dictionary<int, DoorMonitor.DoorInfo>
        ////        {
        ////            {
        ////                BellyDoorId,
        ////                new DoorMonitor.DoorInfo(
        ////                    _accessor.BellyDoorGuid,
        ////                    ApplicationMeters.BellyDoorOpenCount,
        ////                    ApplicationMeters.BellyDoorOpenPowerOffCount)
        ////            }
        ////        };
        ////    _accessor._doors = doors;

        ////    // Set up a queued event for a belly door closed. 
        ////    var doorService = _doorService.Object;
        ////    var logicalDoors = doorService.LogicalDoors;
        ////    var physicalId = logicalDoors[BellyDoorId].PhysicalId;
        ////    ICollection<IEvent> queuedEvents = new Collection<IEvent>();
        ////    queuedEvents.Add(new InputEvent(physicalId, true));
        ////    _iio.Setup(m => m.GetQueuedEvents).Returns(queuedEvents);

        ////    // NOT mocking log confirms that it isn't called
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.BellyDoorGuid,
        ////        "Belly Door is Open",
        ////        "Belly Door Closed");

        ////    _isDoorOpenBitMask = 1 << BellyDoorId;
        ////    int maxStoredMessages = _accessor._maxStoredMessages;
        ////    _persistentStorage.Setup(m => m.CreateBlock(PersistenceLevel.Transient, BlockNameLog, maxStoredMessages))
        ////        .Returns(It.IsAny<IPersistentStorageAccessor>);
        ////    _persistentStorage.Setup(m => m.CreateBlock(PersistenceLevel.Transient, BlockNameIndex, 1))
        ////        .Returns(It.IsAny<IPersistentStorageAccessor>);

        ////    _accessor.CheckForPreviouslyOpenDoors();
        ////    Assert.AreEqual(1, _displayedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.BellyDoorGuid), _displayedMessages));
        ////    Assert.AreEqual(0, _removedMessages.Count);
        ////}

        ////[TestMethod]
        ////public void CheckForOpenDoorsWhenDoorClosedDuringRebootTest()
        ////{
        ////    var theEvent = new OpenEvent(MainDoorId, "Main Door");

        ////    MockLog(false, theEvent.LogicalId, theEvent.Timestamp, true, true);
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.MainDoorGuid,
        ////        "Main Door is Open",
        ////        "Main Door Closed");
        ////    ////_eventBus.Setup(m => m.Publish(It.IsAny<DoorHardTiltEvent>())).Verifiable();
        ////    _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();
        ////    _doorService.Setup(m => m.GetDoorAction(MainDoorId)).Returns(DoorAction.Open);

        ////    _accessor.HandleEvent(theEvent);

        ////    Assert.AreEqual(1, _displayedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.MainDoorGuid), _displayedMessages));

        ////    // Now, simulate door closed when system is down.
        ////    _doorService.Setup(m => m.GetDoorAction(MainDoorId)).Returns(DoorAction.Closed);

        ////    _accessor.CheckForPreviouslyOpenDoors();
        ////    Assert.AreEqual(2, _displayedMessages.Count);
        ////    ////var messageAdded = _displayedMessages[1];
        ////    ////Assert.AreEqual(DisplayableMessageClassification.SoftError, messageAdded.Classification);
        ////}

        ////[TestMethod]
        ////public void CheckForPreviouslyOpenDoorsTestWithQueuedEventsTest()
        ////{
        ////    var doors =
        ////        new Dictionary<int, DoorMonitor.DoorInfo>
        ////        {
        ////            {
        ////                BellyDoorId,
        ////                new DoorMonitor.DoorInfo(
        ////                    _accessor.BellyDoorGuid,
        ////                    ApplicationMeters.BellyDoorOpenCount,
        ////                    ApplicationMeters.DropDoorOpenPowerOffCount)
        ////            }
        ////        };
        ////    _accessor._doors = doors;

        ////    // Set up a queued event for a belly door closed. 
        ////    var doorService = _doorService.Object;
        ////    var logicalDoors = doorService.LogicalDoors;
        ////    var physicalId = logicalDoors[BellyDoorId].PhysicalId;
        ////    ICollection<IEvent> queuedEvents = new Collection<IEvent>();
        ////    queuedEvents.Add(new InputEvent(physicalId, true));
        ////    _iio.Setup(m => m.GetQueuedEvents).Returns(queuedEvents);

        ////    // NOT mocking log confirms that it isn't called
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.BellyDoorGuid,
        ////        "Belly Door is Open",
        ////        "Belly Door Closed");

        ////    _isDoorOpenBitMask = 1 << BellyDoorId;
        ////    int maxStoredMessages = _accessor._maxStoredMessages;
        ////    _persistentStorage.Setup(m => m.CreateBlock(PersistenceLevel.Transient, BlockNameLog, maxStoredMessages))
        ////        .Returns(It.IsAny<IPersistentStorageAccessor>);
        ////    _persistentStorage.Setup(m => m.CreateBlock(PersistenceLevel.Transient, BlockNameIndex, 1))
        ////        .Returns(It.IsAny<IPersistentStorageAccessor>);

        ////    _accessor.CheckForPreviouslyOpenDoors();
        ////    Assert.AreEqual(1, _displayedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorClosedMessageText(_accessor.BellyDoorGuid), _displayedMessages));
        ////    Assert.AreEqual(0, _removedMessages.Count);
        ////}

        ////[TestMethod]
        ////public void CheckForOpenDoorsWhenDoorClosedDuringRebootTest()
        ////{
        ////    var theEvent = new OpenEvent(MainDoorId);

        ////    MockLog(false, theEvent.LogicalId, theEvent.Timestamp, true, true);
        ////    _meters[ApplicationMeters.MainDoorOpenCount].Setup(m => m.Increment(1)).Verifiable();
        ////    MockDisableManagerAndMessageDisplay(
        ////        true,
        ////        _accessor.MainDoorGuid,
        ////        "Main Door is Open",
        ////        "Main Door Closed");
        ////    _eventBus.Setup(m => m.Publish(It.IsAny<DoorHardTiltEvent>())).Verifiable();
        ////    _eventBus.Setup(m => m.Publish(It.IsAny<DoorOpenMeteredEvent>())).Verifiable();
        ////    _doorService.Setup(m => m.GetDoorAction(MainDoorId)).Returns(DoorAction.Open);

        ////    _accessor.HandleEvent(theEvent);

        ////    Assert.AreEqual(1, _displayedMessages.Count);
        ////    Assert.IsTrue(IsMessageInList(FindDoorOpenMessageText(_accessor.MainDoorGuid), _displayedMessages));

        ////    // Now, simulate door closed when system is down.
        ////    _doorService.Setup(m => m.GetDoorAction(MainDoorId)).Returns(DoorAction.Closed);

        ////    _accessor.CheckForPreviouslyOpenDoors();
        ////    Assert.AreEqual(2, _displayedMessages.Count);
        ////    var messageAdded = _displayedMessages[1];
        ////    Assert.AreEqual(DisplayableMessageClassification.SoftError, messageAdded.Classification);
        ////}

        ////[TestMethod]
        ////public void CheckPersistentStorageWrapAroundTest()
        ////{
        ////    _persistentStorage.Setup(m => m.BlockExists(BlockNameLog)).Returns(true).Verifiable();
        ////    _persistentStorage.Setup(m => m.BlockExists(BlockNameIndex)).Returns(true).Verifiable();
        ////    _storageAccessor.SetupGet(m => m["Index"]).Returns(_eventQueue.Count).Verifiable();
        ////    var doorMessages = new List<DoorMessage>();
        ////    for (var i = 0; i < 205; ++i)
        ////    {
        ////        var message = new DoorMessage { DoorId = i };
        ////        doorMessages.Add(message);
        ////    }

        ////    foreach (var each in doorMessages)
        ////    {
        ////        MockLog(true, each.DoorId, each.Time, false, true);
        ////        _storageAccessor.SetupGet(m => m["Index"]).Returns(_eventQueue.Count).Verifiable();

        ////        _accessor.LogToPersistence(each);
        ////    }

        ////    Assert.AreEqual(_accessor.MaxStoredLogMessages, _eventQueue.Count);
        ////    Assert.AreEqual(204, _eventQueue[_accessor.MaxStoredLogMessages - 1].DoorId);
        ////}

        [TestMethod]
        public void CheckPersistentStorageIntegritySucceedTest()
        {
            var testEvent = new ClosedEvent(MainDoorId, "Main Door");
            MockLog(false, testEvent.LogicalId, testEvent.Timestamp, false, true);
            _persistentStorage.Setup(m => m.VerifyIntegrity(true)).Returns(true);
            _iio.Setup(m => m.ResetPhysicalDoorWasOpened(It.IsAny<int>()));

            MockDisableManagerAndMessageDisplay(false, ApplicationConstants.MainDoorGuid,
                "MainDoorIsOpen", "MainDoorClosed");

            _accessor.HandleEvent(testEvent);
        }

        [TestMethod]
        public void DoorInfoConstructorWithUnknownDoorTest()
        {
            var doorInfo = new DoorMonitor.DoorInfo(
                "E73A483C-1555-446D-AC98-4E23D251F6EF",
                "UnknownDoorMeterName",
                "UnknownPowerOffDoorMeterName");
            Assert.IsNull(doorInfo.DoorOpenMessage);
            Assert.IsNull(doorInfo.DoorClosedMessage);
        }

        [TestMethod]
        public void CheckLogicalDoorsWhereLogicalDoorsIsNullTest()
        {
            _doorService.Setup(m => m.LogicalDoors).Returns((Dictionary<int, LogicalDoor>)null);
            _accessor.CheckLogicalDoors();
        }

        [TestMethod]
        public void CheckLogicalDoorsNoDoorsToAddTest()
        {
            _doorService.Setup(m => m.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>());
            _accessor.CheckLogicalDoors();
        }

        [TestMethod]
        public void TestMechanicalAndOpticalDoorPair()
        {
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardDoorOpticsEnabled, It.IsAny<bool>())).Returns(true);
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.HardMetersEnabledKey, It.IsAny<bool>())).Returns(true);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.UniversalInterfaceBoxEnabled, It.IsAny<bool>())).Returns(true);

            _accessor.InitializeDoorEventHandlers();

            MockDisableManagerAndMessageDisplay(
                true,
                ApplicationConstants.MainDoorGuid,
                "Foo",
                "Bar");

            _accessor.OpenDoor(MainDoorId, null);
            Assert.AreEqual(1, _displayedMessages.Count);
            Assert.IsTrue(IsMessageInList("Main Door Mismatch", _displayedMessages));
            Assert.AreEqual(1, _removedMessages.Count);
            Assert.IsTrue(IsMessageInList("Main Door Closed", _removedMessages));

            _accessor.OpenDoor(MainOpticDoorId, null);
            Assert.AreEqual(2, _displayedMessages.Count);
            Assert.IsTrue(IsMessageInList("Main Door is Open", _displayedMessages));

            _displayedMessages.Clear();
            MockDisableManagerAndMessageDisplay(
                false,
                ApplicationConstants.MainDoorGuid,
                "Geoff",
                "George");

            _accessor.CloseDoor(MainDoorId);
            Assert.AreEqual(1, _displayedMessages.Count);
            Assert.IsTrue(IsMessageInList("Main Door Mismatch", _displayedMessages));

            _removedMessages.Clear();
            _accessor.CloseDoor(MainOpticDoorId);
            Assert.AreEqual(2, _displayedMessages.Count);
            Assert.IsTrue(IsMessageInList("Main Door Closed", _displayedMessages));
            Assert.AreEqual(0, _removedMessages.Count);

            _displayedMessages.Clear();
            _accessor.OpenDoor(MainOpticDoorId, null);
            Assert.AreEqual(1, _displayedMessages.Count);
            Assert.IsTrue(IsMessageInList("Main Door Mismatch", _displayedMessages));
            Assert.AreEqual(1, _removedMessages.Count);
            Assert.IsTrue(IsMessageInList("Main Door Closed", _removedMessages));
        }

        private void SetupDoors()
        {
            _accessor._doors = new Dictionary<int, DoorMonitor.DoorInfo>
            {
                {
                    MainDoorId,
                    new DoorMonitor.DoorInfoWithMismatch(
                        ApplicationConstants.MainDoorGuid.ToString(),
                        ApplicationMeters.MainDoorOpenCount,
                        ApplicationMeters.MainDoorOpenPowerOffCount)
                },
                {
                    BellyDoorId,
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.BellyDoorGuid.ToString(),
                        ApplicationMeters.BellyDoorOpenCount,
                        ApplicationMeters.BellyDoorOpenPowerOffCount)
                },
                {
                    LogicDoorId,
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.LogicDoorGuid.ToString(),
                        ApplicationMeters.LogicDoorOpenCount,
                        ApplicationMeters.LogicDoorOpenPowerOffCount)
                },
                {
                    TopBoxDoorId,
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.TopBoxDoorGuid.ToString(),
                        ApplicationMeters.TopBoxDoorOpenCount,
                        ApplicationMeters.TopBoxDoorOpenPowerOffCount)
                },
                {
                    CashDoorId,
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.CashDoorGuid.ToString(),
                        ApplicationMeters.CashDoorOpenCount,
                        ApplicationMeters.CashDoorOpenPowerOffCount)
                },
                {
                    DropDoorId,
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.DropDoorGuid.ToString(),
                        ApplicationMeters.DropDoorOpenCount,
                        ApplicationMeters.DropDoorOpenPowerOffCount)
                },
                {
                    MechanicalMeterDoorId,
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.MechanicalMeterDoorGuid.ToString(),
                        ApplicationMeters.MechanicalMeterDoorOpenCount,
                        ApplicationMeters.MechanicalMeterDoorOpenPowerOffCount)
                },
                {
                    MainOpticDoorId,
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.MainOpticDoorGuid.ToString(),
                        ApplicationMeters.MainOpticDoorOpenCount,
                        ApplicationMeters.MainOpticDoorOpenPowerOffCount)
                },
                {
                    TopBoxOpticDoorId,
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.TopBoxOpticDoorGuid.ToString(),
                        ApplicationMeters.TopBoxOpticDoorOpenCount,
                        ApplicationMeters.TopBoxOpticDoorOpenPowerOffCount)
                },
                {
                    UniversalInterfaceBoxDoorId,
                    new DoorMonitor.DoorInfo(
                        ApplicationConstants.UniversalInterfaceBoxDoorGuid.ToString(),
                        ApplicationMeters.TopBoxDoorOpenCount,
                        ApplicationMeters.TopBoxDoorOpenPowerOffCount)
                }
            };
        }

        /// <summary>
        ///     Mocks logging the door event message to the persistence
        /// </summary>
        /// <param name="blockExists">Indicates whether the block exists</param>
        /// <param name="doorId">The door's id.</param>
        /// <param name="time">The time when the event is fired/</param>
        /// <param name="isOpen">Indicates whether or not the door is open</param>
        /// <param name="isVerified">Indicates whether or not the event was verified</param>
        private void MockLog(bool blockExists, int doorId, DateTime time, bool isOpen, bool isVerified)
        {
            var level = PersistenceLevel.Transient;
            _storageAccessor.Setup(m => m.StartUpdate(true));
            _storageAccessor.Setup(m => m.Commit());
            var storageAccessor2 = new Mock<IPersistentStorageAccessor>();

            if (blockExists)
            {
                _persistentStorage.Setup(m => m.BlockExists(BlockNameLog)).Returns(true).Verifiable();
                _persistentStorage.Setup(m => m.BlockExists(BlockNameIndex)).Returns(true).Verifiable();
                storageAccessor2.SetupGet(m => m["Index"]).Returns(_eventQueue.Count).Verifiable();
                _persistentStorage.Setup(m => m.GetBlock(BlockNameLog)).Returns(_storageAccessor.Object);
                _persistentStorage.Setup(m => m.GetBlock(BlockNameIndex)).Returns(storageAccessor2.Object);
                if (_eventQueue.Count < _accessor.MaxStoredLogMessages)
                {
                    MockIndexedBlock(_eventQueue.Count, doorId, time, isOpen, isVerified);
                }
                else
                {
                    MockIndexedBlock(_accessor.MaxStoredLogMessages - 1, doorId, time, isOpen, isVerified);
                }
            }
            else
            {
                _persistentStorage.Setup(m => m.BlockExists(BlockNameIndex)).Returns(false).Verifiable();
                int maxStoredMessages = _accessor._maxStoredMessages;
                _persistentStorage.Setup(m => m.CreateBlock(level, BlockNameLog, maxStoredMessages))
                    .Returns(_storageAccessor.Object)
                    .Verifiable();
                _persistentStorage.Setup(m => m.CreateBlock(level, BlockNameIndex, 1))
                    .Returns(storageAccessor2.Object)
                    .Verifiable();
                MockIndexedBlock(0, doorId, time, isOpen, isVerified);
            }
        }

        /// <summary>
        ///     Mocks the indexed log entry.
        /// </summary>
        /// <param name="index">The index to where the entry is stored</param>
        /// <param name="doorId">The door's id.</param>
        /// <param name="time">The time when the event is fired/</param>
        /// <param name="isOpen">Indicates whether or not the door is open</param>
        /// <param name="isVerified">Indicates whether or not the event was verified</param>
        private void MockIndexedBlock(int index, int doorId, DateTime time, bool isOpen, bool isVerified)
        {
            var transaction = new Mock<IPersistentStorageTransaction>(MockBehavior.Default);

            transaction.Setup(m => m.AddBlock(It.IsAny<IPersistentStorageAccessor>())).Verifiable();
            transaction.SetupSet(m => m[BlockNameIndex, "Index"] = index);
            transaction.SetupSet(m => m[BlockNameLog, index, "Time"] = time);
            transaction.SetupSet(m => m[BlockNameLog, index, "DoorId"] = doorId).Verifiable();
            transaction.SetupSet(m => m[BlockNameLog, index, "IsOpen"] = isOpen).Verifiable();
            transaction.SetupSet(m => m[BlockNameLog, index, "ValidationPassed"] = isVerified).Verifiable();
            transaction.Setup(m => m.Commit()).Callback(
                () =>
                {
                    var blockEntry = new BlockEntry(doorId, time, isOpen, isVerified);

                    if (_eventQueue.Count < _accessor.MaxStoredLogMessages)
                    {
                        _eventQueue.Add(blockEntry);
                    }
                    else
                    {
                        for (var i = 1; i < _accessor.MaxStoredLogMessages; i++)
                        {
                            _eventQueue[i - 1] = _eventQueue[i];
                        }

                        _eventQueue[_accessor.MaxStoredLogMessages - 1] = new BlockEntry(
                            doorId,
                            time,
                            isOpen,
                            isVerified);
                    }
                });

            transaction.Setup(m => m.Dispose()).Verifiable();

            _storageAccessor.Setup(m => m.StartTransaction()).Returns(transaction.Object);
        }

        /// <summary>
        ///     Mocks the method calls in IDisableManager and IMessageDisplay.
        /// </summary>
        /// <param name="disable">Indicates whether to mock Disable or Enable of IDisableManager.</param>
        /// <param name="doorGuid">The Guid of door.</param>
        /// <param name="openMessage">The message displayed when door is open</param>
        /// <param name="closedMessage">The message displayed when door is closed.</param>
        private void MockDisableManagerAndMessageDisplay(
            bool disable,
            Guid doorGuid,
            string openMessage,
            string closedMessage)
        {
            if (disable)
            {
                if (doorGuid != Guid.Empty && !string.IsNullOrEmpty(openMessage))
                {
                    _disableManager.Setup(
                            m => m.Disable(doorGuid, SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null))
                        .Callback((Guid g, SystemDisablePriority s, Func<string> message, Type t) => { _displayedMessages.Add(message?.Invoke()); }).Verifiable();
                }

                if (!string.IsNullOrEmpty(closedMessage))
                {
                    _messageDisplay.Setup(m => m.RemoveMessage(It.IsAny<DisplayableMessage>()))
                        .Callback((DisplayableMessage message) => { _removedMessages.Add(message.Message); }).Verifiable();
                }
            }
            else
            {
                if (doorGuid != Guid.Empty)
                {
                    _disableManager.Setup(m => m.Enable(doorGuid)).Verifiable();
                    _disableManager.Setup(m => m.CurrentDisableKeys).Returns(new List<Guid>() { doorGuid });
                }

                if (!string.IsNullOrEmpty(openMessage))
                {
                    _messageDisplay.Setup(m => m.RemoveMessage(It.IsAny<DisplayableMessage>()))
                        .Callback((DisplayableMessage message) => { _removedMessages.Add(message.Message); }).Verifiable();
                }

                if (!string.IsNullOrEmpty(closedMessage))
                {
                    _messageDisplay.Setup(m => m.DisplayMessage(It.IsAny<DisplayableMessage>()))
                        .Callback((DisplayableMessage message) => { _displayedMessages.Add(message.Message); }).Verifiable();
                }
            }
        }

        /// <summary>
        ///     Finds the door open message text associated with a given door Guid.
        /// </summary>
        /// <param name="doorGuid">String representation of a Guid to find a match for</param>
        /// <returns>The door open message text associated with a given door Guid; empty if no match</returns>
        private string FindDoorOpenMessageText(string doorGuid)
        {
            var doorInfo = ((Dictionary<string, DoorMonitor.DoorInfo>)_accessor._doorInfo).Values.ToList();

            var matchingDoor = doorInfo.Where(
                x => string.Equals(x.DoorGuid.ToString("B"), doorGuid, StringComparison.CurrentCultureIgnoreCase));
            var doorInfos = matchingDoor as DoorMonitor.DoorInfo[] ?? matchingDoor.ToArray();
            return doorInfos.Any() ? doorInfos.First().DoorOpenMessage.Message : string.Empty;
        }

        /// <summary>
        ///     Finds the door closed message text associated with a given door Guid.
        /// </summary>
        /// <param name="doorGuid">String representation of a Guid to find a match for</param>
        /// <returns>The door closed message text associated with a given door Guid; empty if no match</returns>
        private string FindDoorClosedMessageText(string doorGuid)
        {
            var doorInfo = ((Dictionary<string, DoorMonitor.DoorInfo>)_accessor._doorInfo).Values.ToList();

            var matchingDoor = doorInfo.Where(
                x => string.Equals(x.DoorGuid.ToString("B"), doorGuid, StringComparison.CurrentCultureIgnoreCase));
            var doorInfos = matchingDoor as DoorMonitor.DoorInfo[] ?? matchingDoor.ToArray();
            return doorInfos.Any() ? doorInfos.First().DoorClosedMessage.Message : string.Empty;
        }

        /// <summary>
        ///     Checks whether a message exists in a list.
        /// </summary>
        /// <param name="message">The message to check.</param>
        /// <param name="list">The collection of DisplayableMessage.</param>
        /// <returns>True if the message is in the list; false otherwise.</returns>
        private bool IsMessageInList(string message, List<string> list)
        {
            return list.Any(x => x == message);
        }

        /// <summary>
        ///     Initializes the door meters stored in the test MeterManager.
        /// </summary>
        private void SetupMeters()
        {
            _meterManager.SetupGet(m => m.Meters).Returns(
                _meters.Keys.ToArray());
            foreach (var meter in _meters)
            {
                _meterManager.Setup(m => m.IsMeterProvided(meter.Key)).Returns(true);
                _meterManager.Setup(m => m.GetMeter(meter.Key)).Returns(meter.Value.Object);
                meter.Value.SetupGet(m => m.Classification).Returns(new OccurrenceMeterClassification());
            }
        }

        /// <summary>
        ///     Simulates the entry persisted into the storage.
        /// </summary>
        private struct BlockEntry
        {
            /// <summary>The door's Id.</summary>
            private readonly int _doorId;

            /// <summary>The time when the event was fired.</summary>
            private readonly DateTime _time;

            /// <summary>Indicates whether it's an open event.</summary>
            private readonly bool _isOpen;

            /// <summary>Indicates whether the validation passed.</summary>
            private readonly bool _validationPassed;

            /// <summary>
            ///     Initializes a new instance of the BlockEntry struct.
            /// </summary>
            /// <param name="id">The door's Id</param>
            /// <param name="tm">The timestamp when the event is fired.</param>
            /// <param name="open">Indicates when it is a open event.</param>
            /// <param name="verified">Indicates whether the integrity is verified.</param>
            public BlockEntry(int id, DateTime tm, bool open, bool verified)
            {
                _doorId = id;
                _time = tm;
                _isOpen = open;
                _validationPassed = verified;
            }
        }
    }
}
