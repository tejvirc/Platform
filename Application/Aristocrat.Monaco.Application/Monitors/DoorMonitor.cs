namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using Contracts;
    using Contracts.EdgeLight;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Hardware.Contracts;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.EdgeLighting;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.KeySwitch;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts;
    using Localization;
    using log4net;
    using Monaco.Localization.Properties;
    using Timer = System.Timers.Timer;

    /// <summary>
    ///     Definition of the DoorMonitor class.
    /// </summary>
    public partial class DoorMonitor : IService, IDoorMonitor, IDisposable
    {
        private const string BlockNameLog = "Aristocrat.Monaco.Application.Monitors.DoorMonitor.Log";
        private const string BlockNameIndex = "Aristocrat.Monaco.Application.Monitors.DoorMonitor.Index";

        private const string MessageRedisplayBitMask = "MessageRedisplayBitMask";
        private const string IsDoorOpenBitMask = "IsDoorOpenBitMask";
        private const string IsMeteredBitMask = "IsMeteredBitMask";
        private const string IsVerificationTicketQueuedBitMask = "IsVerificationTicketQueuedBitMask";
        private const string OperatorSwitchName = "Operator Switch";
        private const string ConfigLocation = "/DoorMonitor/Configuration";
        private const string TrueStr = "true";
        private const double CheckLogicalDoorsWaitTimeMs = 1000.0;
        private const byte MaximumVolume = 100;

        private static readonly string BellyDoorGuid = ApplicationConstants.BellyDoorGuid.ToString();
        private static readonly string CashDoorGuid = ApplicationConstants.CashDoorGuid.ToString();
        private static readonly string LogicDoorGuid = ApplicationConstants.LogicDoorGuid.ToString();
        private static readonly string MainDoorGuid = ApplicationConstants.MainDoorGuid.ToString();
        private static readonly string SecondaryCashDoorGuid = ApplicationConstants.SecondaryCashDoorGuid.ToString();
        private static readonly string TopBoxDoorGuid = ApplicationConstants.TopBoxDoorGuid.ToString();
        private static readonly string DropDoorGuid = ApplicationConstants.DropDoorGuid.ToString();
        private static readonly string MechanicalMeterDoorGuid = ApplicationConstants.MechanicalMeterDoorGuid.ToString();
        private static readonly string MainOpticDoorGuid = ApplicationConstants.MainOpticDoorGuid.ToString();
        private static readonly string TopBoxOpticDoorGuid = ApplicationConstants.TopBoxOpticDoorGuid.ToString();
        private static readonly string UniversalInterfaceBoxDoorGuid = ApplicationConstants.UniversalInterfaceBoxDoorGuid.ToString();

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ConcurrentQueue<IEvent> _queuedStartUpEvents = new();
        private readonly string _blockNameStates = "Aristocrat.Monaco.Application.Monitors.DoorMonitor.States";

        private readonly Dictionary<DoorLogicalId, DoorInfo> _doorInfo = new()
        {
            {
                DoorLogicalId.Belly,
                new DoorInfo(
                    BellyDoorGuid,
                    ApplicationMeters.BellyDoorOpenCount,
                    ApplicationMeters.BellyDoorOpenPowerOffCount)
            },
            {
                DoorLogicalId.CashBox,
                new DoorInfo(
                    CashDoorGuid,
                    ApplicationMeters.CashDoorOpenCount,
                    ApplicationMeters.CashDoorOpenPowerOffCount)
            },
            {
                DoorLogicalId.Logic,
                new DoorInfo(
                    LogicDoorGuid,
                    ApplicationMeters.LogicDoorOpenCount,
                    ApplicationMeters.LogicDoorOpenPowerOffCount)
            },
            {
                DoorLogicalId.Main,
                new DoorInfoWithMismatch(
                    MainDoorGuid,
                    ApplicationMeters.MainDoorOpenCount,
                    ApplicationMeters.MainDoorOpenPowerOffCount)
            },
            {
                DoorLogicalId.SecondaryCashBox,
                new DoorInfo(
                    SecondaryCashDoorGuid,
                    ApplicationMeters.SecondaryCashDoorOpenCount,
                    ApplicationMeters.SecondaryCashDoorOpenPowerOffCount)
            },
            {
                DoorLogicalId.TopBox,
                new DoorInfoWithMismatch(
                    TopBoxDoorGuid,
                    ApplicationMeters.TopBoxDoorOpenCount,
                    ApplicationMeters.TopBoxDoorOpenPowerOffCount)
            },
            {
                DoorLogicalId.DropDoor,
                new DoorInfo(
                    DropDoorGuid,
                    ApplicationMeters.DropDoorOpenCount,
                    ApplicationMeters.DropDoorOpenPowerOffCount)
            },
            {
                DoorLogicalId.MechanicalMeter,
                new DoorInfo(
                    MechanicalMeterDoorGuid,
                    ApplicationMeters.MechanicalMeterDoorOpenCount,
                    ApplicationMeters.MechanicalMeterDoorOpenPowerOffCount)
            },
            {
                DoorLogicalId.MainOptic,
                new DoorInfoWithMismatch(
                    MainOpticDoorGuid,
                    ApplicationMeters.MainOpticDoorOpenCount,
                    ApplicationMeters.MainOpticDoorOpenPowerOffCount)
            },
            {
                DoorLogicalId.TopBoxOptic,
                new DoorInfoWithMismatch(
                    TopBoxOpticDoorGuid,
                    ApplicationMeters.TopBoxOpticDoorOpenCount,
                    ApplicationMeters.TopBoxOpticDoorOpenPowerOffCount)
            },
            {
                DoorLogicalId.UniversalInterfaceBox,
                new DoorInfoWithMismatch(
                    UniversalInterfaceBoxDoorGuid,
                    ApplicationMeters.UniversalInterfaceBoxDoorOpenCount,
                    ApplicationMeters.UniversalInterfaceBoxDoorOpenPowerOffCount)
            }
        };

        private readonly List<string> _doorOpenCountMeters = new()
        {
            ApplicationMeters.BellyDoorOpenCount,
            ApplicationMeters.BellyDoorOpenPowerOffCount,
            ApplicationMeters.CashDoorOpenCount,
            ApplicationMeters.CashDoorOpenPowerOffCount,
            ApplicationMeters.LogicDoorOpenCount,
            ApplicationMeters.LogicDoorOpenPowerOffCount,
            ApplicationMeters.MainDoorOpenCount,
            ApplicationMeters.MainDoorOpenPowerOffCount,
            ApplicationMeters.SecondaryCashDoorOpenCount,
            ApplicationMeters.SecondaryCashDoorOpenPowerOffCount,
            ApplicationMeters.TopBoxDoorOpenCount,
            ApplicationMeters.TopBoxDoorOpenPowerOffCount,
            ApplicationMeters.UniversalInterfaceBoxDoorOpenCount,
            ApplicationMeters.UniversalInterfaceBoxDoorOpenPowerOffCount,
            ApplicationMeters.DropDoorOpenCount,
            ApplicationMeters.DropDoorOpenPowerOffCount,
            ApplicationMeters.MechanicalMeterDoorOpenCount,
            ApplicationMeters.MechanicalMeterDoorOpenPowerOffCount,
            ApplicationMeters.MainOpticDoorOpenCount,
            ApplicationMeters.MainOpticDoorOpenPowerOffCount,
            ApplicationMeters.TopBoxOpticDoorOpenCount,
            ApplicationMeters.TopBoxOpticDoorOpenPowerOffCount
        };

        private readonly Dictionary<int, DoorInfo> _doors = new();

        private readonly object _eventLock = new();
        private readonly LinkedList<DoorMessage> _messages = new();
        private readonly object _messagesLock = new();
        private readonly List<DisplayableMessage> _messagesToRedisplay = new();
        private readonly object _messagesToRedisplayLock = new();

        private readonly Dictionary<int, DoorOpticsEventHandler> _handlers = new();

        private IPropertiesManager _propertiesManager;
        private IEventBus _eventBus;
        private ISystemDisableManager _disableManager;
        private IMessageDisplay _messageDisplay;
        private IPersistentStorageManager _persistentStorage;
        private IDoorService _doorService;
        private IAudio _audioService;

        private bool _auditMenuWindowOpen;
        private bool _maintenanceModeActive;

        /// <summary>Indicates whether or not we are delaying the posting of the door open metered event for the logic door.</summary>
        /// <remarks>
        ///     This is used to not queue the verification ticket for printing until all queued door open events
        ///     have been handled so that the verification ticket reflects an accurate door open meter count for any doors that
        ///     may have been opened while powered-off.
        /// </remarks>
        private bool _delayLogicDoorOpenMeteredEvent;

        private bool _disposed;

        private CancellationTokenSource _loadDoorsCancellationToken;
        private bool _logicalDoorsLoaded;
        private int _logicDoorId;
        private int _maxStoredMessages = 200;
        private bool _doorOpenAlarmOperatorCanCancel;
        private bool _doorOpenAlarmCanBeStopped;
        private int _doorOpenAlarmRepeatSeconds;
        private int _doorAlarmLoopCount;
        private Timer _doorOpenAlarmTimer = new();

        /// <summary>The number of queued open events (queued open events or still open on boot-up).</summary>
        /// <remarks>
        ///     This is used to delay the posting of a DoorOpenMeteredEvent for the logic door until all queued open events have
        ///     been processed so that
        ///     an accurate door open meter count is represented on the verification ticket.
        /// </remarks>
        private int _queuedOpenEventCount;

        private IPersistentStorageAccessor _storageAccessorForStates;
        private IEdgeLightToken _edgeLightStateToken;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public event EventHandler<DoorMonitorAppendedEventArgs> DoorMonitorAppended;

        /// <inheritdoc />
        public IList<DoorMessage> Log
        {
            get
            {
                lock (_messagesLock)
                {
                    return new List<DoorMessage>(_messages);
                }
            }
        }

        /// <inheritdoc />
        public Dictionary<int, string> Doors
        {
            get { return _doors.ToDictionary(pair => pair.Key, pair => GetDoorString(pair.Value)); }
        }

        /// <inheritdoc />
        public int MaxStoredLogMessages
        {
            get => _maxStoredMessages;

            set
            {
                _maxStoredMessages = value;
                if (_maxStoredMessages < 0)
                {
                    _maxStoredMessages = 0;
                }

                Logger.Debug($"MaxStoredLogMessages set to {_maxStoredMessages}");
            }
        }

        /// <inheritdoc />
        public Dictionary<int, bool> GetLogicalDoors()
        {
            var result = new Dictionary<int, bool>();
            foreach (var door in _doors)
            {
                result[door.Key] = IsDoorOpen(door.Key);
            }

            return result;
        }

        public string GetLocalizedDoorName(int doorId, bool useDefaultCulture = false)
        {
            var provider = Localizer.For(CultureFor.Operator) as OperatorCultureProvider;
            var culture = useDefaultCulture ? provider.DefaultCulture : provider.CurrentCulture;

            switch (doorId)
            {
                case (int)DoorLogicalId.Main:
                    return provider.GetString(culture, ResourceKeys.MainDoorName);

                case (int)DoorLogicalId.MechanicalMeter:
                    return provider.GetString(culture, ResourceKeys.MechanicalMeterDoorName);

                case (int)DoorLogicalId.Logic:
                    return provider.GetString(culture, ResourceKeys.LogicDoorName);

                case (int)DoorLogicalId.DropDoor:
                    return provider.GetString(culture, ResourceKeys.DropDoorName);

                case (int)DoorLogicalId.TopBox:
                    return provider.GetString(culture, ResourceKeys.TopBoxDoorName);

                case (int)DoorLogicalId.CashBox:
                    return provider.GetString(culture, ResourceKeys.CashDoorName);

                case (int)DoorLogicalId.Belly:
                    return provider.GetString(culture, ResourceKeys.BellyDoorName);

                case (int)DoorLogicalId.MainOptic:
                    return provider.GetString(culture, ResourceKeys.MainOpticDoorName);

                case (int)DoorLogicalId.TopBoxOptic:
                    return provider.GetString(culture, ResourceKeys.TopBoxOpticDoorName);

                case (int)DoorLogicalId.UniversalInterfaceBox:
                    return provider.GetString(ResourceKeys.UniversalInterfaceBoxDoorName);

                default:
                    return _doorService.GetDoorName(doorId);
            }
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IDoorMonitor) };

        /// <inheritdoc />
        public void Initialize()
        {
            Logger.Debug("Starting the DoorMonitor...");

            // This sets up the storage accessor
            CheckLogicalDoors();

            InitializeServices();

            InitializeDoorEventHandlers();
            Subscribe();

            if (!_logicalDoorsLoaded)
            {
                RetryLoadDoors();
            }

            var configurationNodes =
                MonoAddinsHelper.GetSelectedNodes<DoorMonitorConfigurationExtensionNode>(ConfigLocation);
            var doorOpenAlarmNodes =
                MonoAddinsHelper.GetChildNodes<DoorOpenAlarmExtensionNode>(configurationNodes.First());
            
            _doorOpenAlarmRepeatSeconds = Convert.ToInt32(doorOpenAlarmNodes.First().RepeatSeconds);
            if (_doorOpenAlarmRepeatSeconds > 0)
            {
                _doorOpenAlarmTimer.Elapsed += OnDoorOpenAlarmTimeout;
                _doorOpenAlarmTimer.Interval = TimeSpan.FromSeconds(_doorOpenAlarmRepeatSeconds).TotalMilliseconds;
            }

            _doorAlarmLoopCount = Convert.ToInt32(doorOpenAlarmNodes.First().LoopCount);
            _doorOpenAlarmOperatorCanCancel = doorOpenAlarmNodes.First().OperatorCanCancel.Equals(TrueStr);
            var doorOpenAlarmCanBeStopped = doorOpenAlarmNodes.First().CanStopSoundWhenDoorIsClosed;
            _doorOpenAlarmCanBeStopped = string.IsNullOrEmpty(doorOpenAlarmCanBeStopped) || doorOpenAlarmCanBeStopped.Equals(TrueStr);
            Logger.Debug(
                $"Found {ConfigLocation} node RepeatSeconds: {_doorOpenAlarmRepeatSeconds} OperatorCanCancel: {_doorOpenAlarmOperatorCanCancel} DoorAlarmCanBeStopped: {_doorOpenAlarmCanBeStopped}");
            

            CheckDoorAlarm(true, true);
        }

        public void InitializeServices()
        {
            _propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            _disableManager = ServiceManager.GetInstance().GetService<ISystemDisableManager>();
            _messageDisplay = ServiceManager.GetInstance().GetService<IMessageDisplay>();
            _persistentStorage = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
            _doorService = ServiceManager.GetInstance().GetService<IDoorService>();
            _audioService = ServiceManager.GetInstance().GetService<IAudio>();
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (disposing)
            {
                if (_loadDoorsCancellationToken != null)
                {
                    _loadDoorsCancellationToken.Cancel(false);
                    _loadDoorsCancellationToken.Dispose();
                }

                _doorOpenAlarmTimer?.Stop();
                if (_doorOpenAlarmTimer != null)
                {
                    _doorOpenAlarmTimer.Dispose();
                    _doorOpenAlarmTimer = null;
                }

                _eventBus.UnsubscribeAll(this);
            }

            _loadDoorsCancellationToken = null;
        }

        private static string GetDoorString(DoorInfo di)
        {
            switch (di.DoorMeterName)
            {
                case ApplicationMeters.BellyDoorOpenCount:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BellyDoorName);
                case ApplicationMeters.CashDoorOpenCount:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CashDoorName);
                case ApplicationMeters.LogicDoorOpenCount:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LogicDoorName);
                case ApplicationMeters.MainDoorOpenCount:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MainDoorName);
                case ApplicationMeters.SecondaryCashDoorOpenCount:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SecondaryCashDoorName);
                case ApplicationMeters.TopBoxDoorOpenCount:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TopBoxDoorName);
                case ApplicationMeters.UniversalInterfaceBoxDoorOpenCount:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UniversalInterfaceBoxDoorName);
                case ApplicationMeters.DropDoorOpenCount:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DropDoorName);
                case ApplicationMeters.MechanicalMeterDoorOpenCount:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MechanicalMeterDoorName);
                case ApplicationMeters.MainOpticDoorOpenCount:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MainOpticDoorName);
                case ApplicationMeters.TopBoxOpticDoorOpenCount:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TopBoxOpticDoorName);
                default:
                    return "Unknown Door";
            }
        }

        private static void Retry(TimeSpan pollInterval, Func<bool> action, CancellationToken token)
        {
            Task.Run(
                async
                    () =>
                {
                    while (true)
                    {
                        if (token.IsCancellationRequested || action())
                        {
                            break;
                        }

                        await Task.Delay(pollInterval, token);
                    }
                },
                token);
        }

        public void InitializeDoorEventHandlers()
        {
            var doorOptic = _propertiesManager.GetValue(ApplicationConstants.ConfigWizardDoorOpticsEnabled, false);
            if (!doorOptic)
            {
                _doorService.IgnoredDoors.Add((int)DoorLogicalId.MainOptic);
                _doorService.IgnoredDoors.Add((int)DoorLogicalId.TopBoxOptic);
            }
            else
            {
                var mainOpticDoorHandler = new DoorOpticsEventHandler(
                    (int)DoorLogicalId.Main, (int)DoorLogicalId.MainOptic,
                    _disableManager, _messageDisplay);
                _handlers.Add((int)DoorLogicalId.Main, mainOpticDoorHandler);
                _handlers.Add((int)DoorLogicalId.MainOptic, mainOpticDoorHandler);

                var topOpticDoorHandler = new DoorOpticsEventHandler(
                    (int)DoorLogicalId.TopBox, (int)DoorLogicalId.TopBoxOptic,
                    _disableManager, _messageDisplay);
                _handlers.Add((int)DoorLogicalId.TopBox, topOpticDoorHandler);
                _handlers.Add((int)DoorLogicalId.TopBoxOptic, topOpticDoorHandler);
            }

            var hardMetersEnabled = _propertiesManager.GetValue(HardwareConstants.HardMetersEnabledKey, false);
            if (!hardMetersEnabled)
            {
                _doorService.IgnoredDoors.Add((int)DoorLogicalId.MechanicalMeter);
            }

            var universalInterfaceBoxEnabled = _propertiesManager.GetValue(ApplicationConstants.UniversalInterfaceBoxEnabled, false);
            if (!universalInterfaceBoxEnabled)
            {
                _doorService.IgnoredDoors.Add((int)DoorLogicalId.UniversalInterfaceBox);
            }
        }

        private void OnDoorOpenAlarmTimeout(object sender, ElapsedEventArgs e)
        {
            if (!ServiceManager.GetInstance().IsServiceAvailable<IAudio>())
            {
                Logger.Error("Unable to operate Door Alarm.  Service unavailable.");
                return;
            }

            if (!_audioService.IsPlaying())
            {
                var alertVolume = _propertiesManager.GetValue(ApplicationConstants.AlertVolumeKey, (byte)100);
                _audioService.Play(SoundName.Alarm, _doorAlarmLoopCount, alertVolume);
                Logger.Debug($"Door open alarm timer timed-out, playing alarm");
            }
            else
            {
                Logger.Debug($"Door open alarm timer timed-out, playing alarm");
            }
        }

        private void CheckDoorAlarm(bool doorWasOpened, bool isBoot)
        {
            var doorAlarmEnabled = _propertiesManager.GetValue(HardwareConstants.DoorAlarmEnabledKey, true);
            if (!doorAlarmEnabled)
            {
                return;
            }

            var numDoorsOpen = GetNumDoorsOpen();
            if (doorWasOpened)
            {
                if (!_auditMenuWindowOpen && !_maintenanceModeActive &&
                    (numDoorsOpen > 0 || numDoorsOpen > 1 && isBoot))
                {
                    PlayOpenDoorAlarm();
                    Logger.DebugFormat(
                        "Door open alarm played with {0} open door{1}",
                        numDoorsOpen,
                        numDoorsOpen > 1 ? "s" : string.Empty);
                }
            }
            else
            {
                if (numDoorsOpen == 0)
                {
                    StopOpenDoorAlarm();
                    Logger.Debug("All doors closed, door open alarm stopped");
                }
            }
        }

        private void PlayOpenDoorAlarm()
        {
            if (ServiceManager.GetInstance().IsServiceAvailable<IAudio>())
            {
                var alertVolume = _propertiesManager.GetValue(ApplicationConstants.AlertVolumeKey, (byte)100);
                var logicDoorFullVolume = _propertiesManager.GetValue(ApplicationConstants.SoundConfigurationLogicDoorFullVolumeAlert, false);
                var isInspectionMode = _propertiesManager.GetValue(KernelConstants.IsInspectionOnly, false);

                //Using door service, since it appears that this is called before the door state is changed, so it will be false on door open and true on door close
                if (logicDoorFullVolume && _doorService.GetDoorOpen((int)DoorLogicalId.Logic))
                {
                    alertVolume = MaximumVolume;
                }
                if (!isInspectionMode)
                {
                    _audioService.Play(SoundName.Alarm, _doorAlarmLoopCount, alertVolume);
                }
                if (_doorOpenAlarmRepeatSeconds > 0)
                {
                    _doorOpenAlarmTimer.Start();
                    Logger.Debug(
                        $"Door open alarm repeat timer started with {_doorOpenAlarmRepeatSeconds} second interval");
                }
            }
            else
            {
                Logger.Error("Unable to play Door Alarm.  Service unavailable.");
            }
        }

        private uint GetNumDoorsOpen()
        {
            uint numDoorsOpen = 0;
            var serviceManager = ServiceManager.GetInstance();

            if (serviceManager.IsServiceAvailable<IDoorService>())
            {
                var doorList = _doorService.LogicalDoors;

                foreach (var pair in doorList)
                {
                    if (!_doorService.GetDoorClosed(pair.Key))
                    {
                        numDoorsOpen++;
                    }
                }
            }
            else
            {
                Logger.Error("Unable to get num doors open.  Service unavailable.");
            }

            return numDoorsOpen;
        }

        private void StopOpenDoorAlarm()
        {
            if (!_doorOpenAlarmCanBeStopped)
            {
                Logger.Debug($"Door open alarm can't be stopped.");
                return;
            }

            if (!ServiceManager.GetInstance().IsServiceAvailable<IAudio>())
            {
                Logger.Error("Unable to stop door alarm.  Service unavailable.");
                return;
            }

            if (_audioService.IsPlaying())
            {
                _audioService.Stop(SoundName.Alarm);
            }

            if (_doorOpenAlarmRepeatSeconds > 0)
            {
                _doorOpenAlarmTimer.Stop();
            }
        }

        private void RetryLoadDoors()
        {
            _loadDoorsCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            try
            {
                if (!_loadDoorsCancellationToken.IsCancellationRequested)
                {
                    Retry(
                        TimeSpan.FromMilliseconds(CheckLogicalDoorsWaitTimeMs),
                        () =>
                        {
                            Logger.Info("Retrying check logical doors");
                            CheckLogicalDoors();
                            return _logicalDoorsLoaded;
                        },
                        _loadDoorsCancellationToken.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Debug("Wait for door load cancelled due to timeout or success");
            }
            catch (Exception e)
            {
                Logger.Error("Exception while checking network change commit", e);
            }
            finally
            {
                _loadDoorsCancellationToken?.Dispose();
                _loadDoorsCancellationToken = null;
            }
        }

        private void CloseDoor(int logicalId)
        {
            // Don't do anything if we already knew this door was closed
            if (IsDoorOpen(logicalId))
            {
                // Clear the is door open bit for this logical door
                SetDoorOpenBit(logicalId, false);

                // If we have a door pair (mechanical and optic) for this door, let the special
                // handler take care of that.
                if (_handlers.ContainsKey(logicalId))
                {
                    _handlers[logicalId].CloseDoor(logicalId, this);
                }
                else
                {
                    var door = _doors[logicalId];

                    // Remove the lockup for this door
                    _disableManager.Enable(door.DoorGuid);

                    // Create a message saying "XYZ door closed"
                    _messageDisplay.DisplayMessage(door.DoorClosedMessage);
                }
            }
        }

        private void OpenDoor(int logicalId, Type eventType)
        {
            // Note that we always treat a door opening as a new event, even if we think it was
            // already open

            // Set the is door open bit for this logical door
            SetDoorOpenBit(logicalId, true);

            // If we have a door pair (mechanical and optic) for this door, let the special
            // handler take care of that.
            if (_handlers.ContainsKey(logicalId))
            {
                _handlers[logicalId].OpenDoor(logicalId, this);
            }
            else
            {
                var door = _doors[logicalId];

                // Create the lockup for this door
                _disableManager.Disable(
                    door.DoorGuid,
                    SystemDisablePriority.Immediate,
                    door.DoorOpenMessage.MessageCallback,
                    eventType);

                // Remove any message saying "XYZ door closed"
                _messageDisplay.RemoveMessage(door.DoorClosedMessage);
            }
        }

        private void CheckForPreviouslyOpenDoors()
        {
            var serviceManager = ServiceManager.GetInstance();
            var doorList = _doorService.LogicalDoors;

            _queuedOpenEventCount = 0;

            try
            {
                var queuedEvents = serviceManager.GetService<IIO>().GetQueuedEvents.OfType<InputEvent>().ToList();
                foreach (var pair in doorList)
                {
                    if (_doors.ContainsKey(pair.Key))
                    {
                        _queuedOpenEventCount += queuedEvents.Count(
                            inEvent => pair.Value.PhysicalId == inEvent.Id && inEvent.Action);

                        if (_doorService.GetDoorClosed(pair.Key))
                        {
                            if (IsDoorOpen(pair.Key))
                            {
                                Logger.Info(
                                    $"{pair.Value.Name} previously open, now closed prior to or during startup...");
                                _messageDisplay.DisplayMessage(_doors[pair.Key].DoorClosedMessage);

                                var append =
                                    queuedEvents.LastOrDefault(evt => pair.Value.PhysicalId == evt.Id)?.Action ?? true;

                                if (append)
                                {
                                    Logger.Debug($"ClosedEvent appended to message list for {pair.Value.Name})");
                                    HandleEvent(new ClosedEvent(pair.Key, true, _doorService.GetDoorName(pair.Key)));
                                }
                            }
                            else
                            {
                                Logger.Info(
                                    $"{pair.Value.Name} (logical door {pair.Key}) closed at startup and last known status was closed");
                            }
                        }
                        else
                        {
                            Logger.Info(
                                $"{pair.Value.Name} (logical door {pair.Key}) previously open, still open prior to or during startup...");
                        }
                    }
                    else
                    {
                        Logger.Warn($"No door in DoorMonitor for {pair.Value.Name} at startup");
                    }
                }
            }
            catch (NullReferenceException e)
            {
                Logger.Error($"{e.GetType().Name} while checking for open doors");
            }
        }

        //If the Note Acceptor is disabled then opening / closing the Note Acceptor Door should not create the lockup.
        private bool CashBoxSetupForCashBoxDoorEvents(int logicalId)
        {
            if (logicalId == (int)DoorLogicalId.CashBox)
            {
                var device = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
                if (device == null) return false;
            }

            return true;
        }

        private void HandleEvent(ClosedEvent theEvent)
        {
            if (!CashBoxSetupForCashBoxDoorEvents(theEvent.LogicalId)) return;

            lock (_eventLock)
            {
                if (!_logicalDoorsLoaded)
                {
                    Logger.Debug($"Handled {theEvent.GetType()} while waiting for logical doors to load...");
                    _queuedStartUpEvents.Enqueue(theEvent);
                    return;
                }

                Logger.Debug($"Handling {theEvent.GetType()} logical id {theEvent.LogicalId}...");

                if (_doors.ContainsKey(theEvent.LogicalId))
                {
                    var verification = false;

                    //If belly door is disabled, disable the EGM as this is a belly door discrepancy.
                    if (theEvent.LogicalId == (int)DoorLogicalId.Belly)
                    {
                        var bellyPanelDoorEnabled = _propertiesManager.GetValue(ApplicationConstants.ConfigWizardBellyPanelDoorEnabled, true);

                        if (!bellyPanelDoorEnabled)
                        {
                            _disableManager.Disable(
                                ApplicationConstants.BellyDoorDiscrepencyGuid,
                                SystemDisablePriority.Immediate,
                                () => Localizer.ForLockup().GetString(ResourceKeys.BellyDoorDiscrepancy));

                            _eventBus.Publish(new BellyDoorDiscrepancyEvent());
                        }
                    }

                    if (_persistentStorage.VerifyIntegrity(true))
                    {
                        Logger.Info("Persistent storage verification passed.");
                        verification = true;
                    }
                    else
                    {
                        Logger.Error("Persistent storage verification failed.");
                    }

                    AppendToMessageList(theEvent, false, verification);

                    var io = ServiceManager.GetInstance().GetService<IIO>();
                    io.ResetPhysicalDoorWasOpened(_doorService.LogicalDoors[theEvent.LogicalId].PhysicalId);

                    CloseDoor(theEvent.LogicalId);

                    Logger.Debug($"Posting DoorClosedMeteredEvent for logical door id {theEvent.LogicalId} ...");
                    _eventBus.Publish(
                        new DoorClosedMeteredEvent(theEvent.LogicalId, theEvent.WhilePoweredDown, theEvent.DoorName));
                }
                else
                {
                    Logger.Error($"Unknown door closed event {theEvent}");
                }

                if (GetNumDoorsOpen() == 0)
                {
                    ServiceManager.GetInstance().GetService<IEdgeLightingStateManager>()
                        .ClearState(_edgeLightStateToken);
                    _edgeLightStateToken = null;
                }
            }

            CheckDoorAlarm(false, false);
        }

        private void HandleEvent(OpenEvent theEvent)
        {
            if (!CashBoxSetupForCashBoxDoorEvents(theEvent.LogicalId)) return;

            //If belly door is disabled, do not create lockup on belly door open.
            //If belly door discrepancy lockup is engaged, disable the lockup.
            if (theEvent.LogicalId == (int)DoorLogicalId.Belly)
            {
                var bellyPanelDoorEnabled = _propertiesManager.GetValue(ApplicationConstants.ConfigWizardBellyPanelDoorEnabled, true);

                if (!bellyPanelDoorEnabled)
                {
                    if (_disableManager.CurrentDisableKeys.Contains(ApplicationConstants.BellyDoorDiscrepencyGuid))
                        _disableManager.Enable(ApplicationConstants.BellyDoorDiscrepencyGuid);

                    return;
                }
            }

            CheckDoorAlarm(true, false);

            lock (_eventLock)
            {
                if (_edgeLightStateToken == null)
                {
                    _edgeLightStateToken = ServiceManager.GetInstance().GetService<IEdgeLightingStateManager>()
                        .SetState(EdgeLightState.DoorOpen);
                }

                if (!_logicalDoorsLoaded)
                {
                    Logger.Debug($"Handled {theEvent.GetType()} while waiting for logical doors to load...");
                    _queuedStartUpEvents.Enqueue(theEvent);
                    return;
                }

                var powered = theEvent.WhilePoweredDown ? "down" : "up";
                Logger.Debug(
                    $"Handling {theEvent.GetType()} logical id {theEvent.LogicalId} while powered {powered}...");

                if (_doors.ContainsKey(theEvent.LogicalId))
                {
                    if (!IsDoorOpen(theEvent.LogicalId))
                    {
                        Logger.Debug($"!IsDoorOpen {theEvent.LogicalId}");

                        // Append door open message to list.
                        AppendToMessageList(theEvent, true, true);

                        if ((long)_storageAccessorForStates[IsDoorOpenBitMask] == 0L &&
                            (!theEvent.WhilePoweredDown || IsDoorOpenOnPowerUp(theEvent.LogicalId)))
                        {
                            if (_delayLogicDoorOpenMeteredEvent || _queuedOpenEventCount > 0)
                            {
                                Logger.Debug(
                                    $"Found _queuedOpenEventCount {_queuedOpenEventCount} _delayLogicDoorOpenMeteredEvent {_delayLogicDoorOpenMeteredEvent} for door open hard tilt, resetting...");
                                _queuedOpenEventCount = 0;
                                _delayLogicDoorOpenMeteredEvent = false;
                            }
                        }

                        // Clear the verification tickets queued bit, sync the lifetime meter (so that this door open
                        // meter will increment), and clear the is metered bit.
                        SetOrClearIsVerificationTicketQueuedBit(theEvent.LogicalId, false);
                        SyncLifetimeMeter(theEvent.LogicalId, theEvent.WhilePoweredDown);
                        SetOrClearIsMeteredBit(theEvent.LogicalId, false);

                        // Set the is door open bit for this logical door.
                        SetDoorOpenBit(theEvent.LogicalId, true);
                        Logger.Debug($"IsDoorOpen {theEvent.LogicalId} set");

                        IncrementDoorMeter(theEvent.LogicalId, theEvent.WhilePoweredDown, theEvent.DoorName);
                    }
                    else if (!IsMetered(theEvent.LogicalId))
                    {
                        Logger.Debug($"!IsMetered {theEvent.LogicalId}");

                        SetOrClearIsVerificationTicketQueuedBit(theEvent.LogicalId, false);

                        IncrementDoorMeter(theEvent.LogicalId, theEvent.WhilePoweredDown, theEvent.DoorName);
                    }
                    else if (!IsVerificationTicketQueued(theEvent.LogicalId))
                    {
                        Logger.Debug($"!IsVerificationTicketQueued {theEvent.LogicalId}");

                        SyncLifetimeMeter(theEvent.LogicalId, theEvent.WhilePoweredDown);

                        PostDoorOpenMeteredEvent(
                            theEvent.LogicalId,
                            theEvent.WhilePoweredDown,
                            true,
                            theEvent.DoorName);
                    }

                    if (!theEvent.WhilePoweredDown || IsDoorOpenOnPowerUp(theEvent.LogicalId))
                    {
                        OpenDoor(theEvent.LogicalId, theEvent.GetType());
                    }
                }
                else
                {
                    Logger.Error($"Unknown door open event {theEvent}");
                }
            }
        }

        private void PostDoorOpenMeteredEvent(int logicalId, bool whilePoweredDown, bool isRecovery, string doorName)
        {
            if (logicalId.Equals(_logicDoorId))
            {
                if (_queuedOpenEventCount > 0)
                {
                    // Set to delay the posting of the door open metered event for the logic door
                    // so that we do not queue the verification ticket for printing until all door open
                    // events have been handled.
                    _delayLogicDoorOpenMeteredEvent = true;
                }
            }

            if (!_delayLogicDoorOpenMeteredEvent || !logicalId.Equals(_logicDoorId))
            {
                Logger.Debug($"Posting DoorOpenMeteredEvent for logical door id {logicalId} ...");
                _eventBus.Publish(new DoorOpenMeteredEvent(logicalId, whilePoweredDown, isRecovery, doorName));
            }

            if (_queuedOpenEventCount > 0)
            {
                Logger.Debug($"Decrementing _queuedOpenEventCount {_queuedOpenEventCount}...");
                _queuedOpenEventCount--;
            }

            if (_delayLogicDoorOpenMeteredEvent && _queuedOpenEventCount == 0)
            {
                _delayLogicDoorOpenMeteredEvent = false;
                Logger.Debug($"Posting DoorOpenMeteredEvent for logical door id {_logicDoorId} ...");
                _eventBus.Publish(new DoorOpenMeteredEvent(_logicDoorId, whilePoweredDown, isRecovery, doorName));
            }
        }

        private bool IsDoorOpenOnPowerUp(int logicalId)
        {
            var door = _doorService.LogicalDoors
                .Single(d => d.Key == logicalId);

            var inputs = ServiceManager.GetInstance().GetService<IIO>().GetInputs;

            return ((long)inputs & ((long)1 << door.Value.PhysicalId)) != 0;
        }

        private void HandleEvent(OffEvent offEvent)
        {
            if (!_logicalDoorsLoaded)
            {
                Logger.Debug($"Handled {offEvent.GetType()} while waiting for logical doors to load...");
                _queuedStartUpEvents.Enqueue(offEvent);
                return;
            }

            var operatorSwitchId = -1;
            if (ServiceManager.GetInstance().IsServiceAvailable<IKeySwitch>())
            {
                var keySwitch = ServiceManager.GetInstance().GetService<IKeySwitch>();

                operatorSwitchId = keySwitch.GetKeySwitchId(OperatorSwitchName);
            }

            if (_doorOpenAlarmOperatorCanCancel)
            {
                if (offEvent.LogicalId == operatorSwitchId)
                {
                    var numDoorsOpen = GetNumDoorsOpen();

                    if (numDoorsOpen > 0)
                    {
                        StopOpenDoorAlarm();
                        Logger.Debug($"Operator cancelled door open alarm with {numDoorsOpen} open doors");
                    }
                }
            }
        }

        private void HandleEvent(MessageRemovedEvent theEvent)
        {
            lock (_eventLock)
            {
                if (!_logicalDoorsLoaded)
                {
                    Logger.Debug($"Handled {theEvent.GetType()} while waiting for logical doors to load...");
                    _queuedStartUpEvents.Enqueue(theEvent);
                    return;
                }

                Logger.Debug($"Handling {theEvent.GetType()}...");

                foreach (var logicalId in _doors.Keys)
                {
                    if (ReferenceEquals(_doors[logicalId].DoorClosedMessage, theEvent.Message))
                    {
                        SetOrClearMessageRedisplayBit(logicalId, false);
                        break;
                    }
                }
            }
        }

        private void LogToPersistence(DoorMessage eventData)
        {
            const PersistenceLevel level = PersistenceLevel.Transient;

            IPersistentStorageAccessor block;

            Logger.Debug(
                $"Adding entry to persistence: {eventData.DoorId} {eventData.Time} {eventData.IsOpen} {eventData.ValidationPassed}");

            var index = 0;

            // check if there is an existing block with this name
            var blockIndex = _persistentStorage.BlockExists(BlockNameIndex)
                ? _persistentStorage.GetBlock(BlockNameIndex)
                : _persistentStorage.CreateBlock(level, BlockNameIndex, 1);

            if (_persistentStorage.BlockExists(BlockNameLog))
            {
                block = _persistentStorage.GetBlock(BlockNameLog);
                index = ((int)blockIndex["Index"] + 1) % MaxStoredLogMessages;
            }
            else
            {
                block = _persistentStorage.CreateBlock(level, BlockNameLog, MaxStoredLogMessages);
            }

            using (var transaction = block.StartTransaction())
            {
                transaction.AddBlock(blockIndex);
                transaction[BlockNameIndex, "Index"] = index;
                transaction[BlockNameLog, index, "Time"] = eventData.Time;
                transaction[BlockNameLog, index, "DoorId"] = eventData.DoorId;
                transaction[BlockNameLog, index, "IsOpen"] = eventData.IsOpen;
                transaction[BlockNameLog, index, "ValidationPassed"] = eventData.ValidationPassed;
                transaction.Commit();
            }

            Logger.Info($"Logging event to persistence... complete! Index is {index}");
        }

        private void AppendToMessageList(DoorBaseEvent doorEvent, bool isOpen, bool isVerified)
        {
            if (!isOpen)
            {
                SetOrClearMessageRedisplayBit(doorEvent.LogicalId, true);
            }

            var doorMessage = default(DoorMessage);
            doorMessage.DoorId = doorEvent.LogicalId;
            doorMessage.Time = doorEvent.Timestamp;
            doorMessage.IsOpen = isOpen;
            doorMessage.ValidationPassed = isVerified;

            if (MaxStoredLogMessages > 0)
            {
                lock (_messagesLock)
                {
                    _messages.AddLast(doorMessage);
                    if (_messages.Count > MaxStoredLogMessages)
                    {
                        _messages.RemoveFirst();
                    }
                }
            }

            DoorMonitorAppended?.Invoke(this, new DoorMonitorAppendedEventArgs(doorMessage));

            LogToPersistence(doorMessage);
        }

        private void CheckLogicalDoors()
        {
            if (_doorService != null)
            {
                try
                {
                    var doorList = _doorService.LogicalDoors;
                    if (doorList.Count > 0)
                    {
                        foreach (var pair in doorList)
                        {
                            var logicalId = pair.Key;
                            var doorName = pair.Value.Name;
                            if (_doorInfo.TryGetValue((DoorLogicalId)logicalId, out var doorInfo))
                            {
                                Logger.Debug($"Adding logical door Id {logicalId} for door {doorName}...");
                                _doors.Add(logicalId, doorInfo);
                                if (doorName.Equals(
                                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LogicDoorName)))
                                {
                                    _logicDoorId = logicalId;
                                }
                            }
                        }
                    }
                    else
                    {
                        Logger.Warn("No logical doors to add");
                        return;
                    }
                }
                catch (NullReferenceException e)
                {
                    Logger.Error($"{e.GetType().Name}: failed to access Door component.");
                }

                // Obtain the log of door events from persistent storage
                // check if there is an existing block with this name
                if (_persistentStorage.BlockExists(BlockNameIndex) && _persistentStorage.BlockExists(BlockNameLog))
                {
                    // Collect data with Count (meaning order), then sort by count asc, to match expected order.
                    var records = new List<Tuple<int, DoorMessage>>(MaxStoredLogMessages);

                    var blockIndex = _persistentStorage.GetBlock(BlockNameIndex);
                    var block = _persistentStorage.GetBlock(BlockNameLog);
                    var latestIndex = (int)blockIndex["Index"];

                    var logs = block.GetAll();

                    for (var i = 0; i < MaxStoredLogMessages; i++)
                    {
                        if (!logs.TryGetValue(i, out var log))
                        {
                            break;
                        }

                        var time = (DateTime)log["Time"];
                        if (time == default(DateTime))
                        {
                            break;
                        }

                        var doorMessage =
                            new DoorMessage
                            {
                                Time = time,
                                DoorId = (int)log["DoorId"],
                                IsOpen = (bool)log["IsOpen"],
                                ValidationPassed = (bool)log["ValidationPassed"]
                            };

                        // i is the physical order in storage.
                        // (i - latestIndex - 1) % MaxStoredLogMessages gives a chronological index for sorting
                        records.Add(Tuple.Create((i - latestIndex - 1) % MaxStoredLogMessages, doorMessage));
                    }

                    lock (_messagesLock)
                    {
                        foreach (var record in records.OrderBy(r => r.Item1).Select(r => r.Item2))
                        {
                            _messages.AddLast(record);
                        }
                    }
                }

                // Create or recover a bit mask that indicates which "Door was Opened" messages need to be redisplayed on boot-up
                var bitMask = 0L;
                if (_persistentStorage.BlockExists(_blockNameStates))
                {
                    _storageAccessorForStates = _persistentStorage.GetBlock(_blockNameStates);
                    bitMask = (long)_storageAccessorForStates[MessageRedisplayBitMask];
                    foreach (var logicalId in _doors.Keys)
                    {
                        long checkBit = 1 << logicalId;
                        if ((bitMask & checkBit) != 0)
                        {
                            _messagesToRedisplay.Add(_doors[logicalId].DoorClosedMessage);
                        }
                    }
                }
                else
                {
                    _storageAccessorForStates = _persistentStorage.CreateBlock(
                        PersistenceLevel.Critical,
                        _blockNameStates,
                        1);
                    _storageAccessorForStates[MessageRedisplayBitMask] = bitMask;
                    _storageAccessorForStates[IsDoorOpenBitMask] = bitMask;
                    _storageAccessorForStates[IsMeteredBitMask] = bitMask;
                    _storageAccessorForStates[IsVerificationTicketQueuedBitMask] = bitMask;
                    foreach (var meter in _doorOpenCountMeters)
                    {
                        _storageAccessorForStates[meter] = 0L;
                    }
                }

                // Set logical doors open.
                _logicalDoorsLoaded = true;

                // Re-display any messages.
                foreach (var message in _messagesToRedisplay)
                {
                    _messageDisplay.DisplayMessage(message);
                }

                _messagesToRedisplay.Clear();

                // Check for previously open doors.
                CheckForPreviouslyOpenDoors();

                while (_queuedStartUpEvents.TryDequeue(out var startUpEvent))
                {
                    ProcessStartUpEvents(startUpEvent);
                }
            }
            else
            {
                Logger.Warn("DoorService unavailable");
            }
        }

        private void ProcessStartUpEvents(IEvent startUpEvent)
        {
            switch (startUpEvent)
            {
                case OpenEvent openEvent:
                    HandleEvent(openEvent);
                    break;
                case ClosedEvent closedEvent:
                    HandleEvent(closedEvent);
                    break;
                case MessageRemovedEvent messageRemovedEvent:
                    HandleEvent(messageRemovedEvent);
                    break;
                case OffEvent offEvent:
                    HandleEvent(offEvent);
                    break;
            }
        }

        private void IncrementDoorMeter(int logicalId, bool whilePoweredDown, string doorName)
        {
            var meterManager = ServiceManager.GetInstance().GetService<IMeterManager>();
            var door = _doors[logicalId];

            var meter = whilePoweredDown ? door.PowerOffMeterName : door.DoorMeterName;

            if (meterManager.IsMeterProvided(meter))
            {
                if ((long)_storageAccessorForStates[meter] == meterManager.GetMeter(meter).Lifetime)
                {
                    Logger.Debug($"Incrementing {meter}...");
                    meterManager.GetMeter(meter).Increment(1);
                }
                else
                {
                    Logger.Debug($"{meter} already incremented {_storageAccessorForStates[meter]}");
                }

                // Set the is metered bit.
                SetOrClearIsMeteredBit(logicalId, true);

                Logger.Debug($"IsMetered {logicalId} set");

                // Sync the saved lifetime meter.
                SyncLifetimeMeter(logicalId, whilePoweredDown);

                // Post the door opened metered event.
                PostDoorOpenMeteredEvent(logicalId, whilePoweredDown, false, doorName);
            }
            else
            {
                Logger.Warn($"Attempt to increment unprovided door meter {meter}");
            }
        }

        private bool IsDoorOpen(int logicalId)
        {
            var bitMask = (long)_storageAccessorForStates[IsDoorOpenBitMask];
            long checkBit = 1 << logicalId;

            return (bitMask & checkBit) > 0;
        }

        private bool IsMetered(int logicalId)
        {
            var bitMask = (long)_storageAccessorForStates[IsMeteredBitMask];
            long checkBit = 1 << logicalId;

            return (bitMask & checkBit) > 0;
        }

        private bool IsVerificationTicketQueued(int logicalId)
        {
            var bitMask = (long)_storageAccessorForStates[IsVerificationTicketQueuedBitMask];
            long checkBit = 1 << logicalId;

            return (bitMask & checkBit) > 0;
        }

        private void SetDoorOpenBit(int logicalId, bool set)
        {
            var bitMask = (long)_storageAccessorForStates[IsDoorOpenBitMask];
            long setOrClearBit = 1 << logicalId;
            if (set)
            {
                bitMask |= setOrClearBit;
            }
            else
            {
                bitMask &= ~setOrClearBit;
            }

            _storageAccessorForStates[IsDoorOpenBitMask] = bitMask;
        }

        private void SetOrClearIsMeteredBit(int logicalId, bool set)
        {
            var bitMask = (long)_storageAccessorForStates[IsMeteredBitMask];
            long setOrClearBit = 1 << logicalId;
            if (set)
            {
                bitMask |= setOrClearBit;
            }
            else
            {
                bitMask &= ~setOrClearBit;
            }

            _storageAccessorForStates[IsMeteredBitMask] = bitMask;
        }

        private void SetOrClearIsVerificationTicketQueuedBit(int logicalId, bool set)
        {
            var bitMask = (long)_storageAccessorForStates[IsVerificationTicketQueuedBitMask];
            long setOrClearBit = 1 << logicalId;
            if (set)
            {
                bitMask |= setOrClearBit;
            }
            else
            {
                bitMask &= ~setOrClearBit;
            }

            _storageAccessorForStates[IsVerificationTicketQueuedBitMask] = bitMask;
        }

        private void SetOrClearMessageRedisplayBit(int logicalId, bool set)
        {
            lock (_messagesToRedisplayLock)
            {
                var bitMask = (long)_storageAccessorForStates[MessageRedisplayBitMask];
                long setOrClearBit = 1 << logicalId;
                if (set)
                {
                    bitMask |= setOrClearBit;
                }
                else
                {
                    bitMask &= ~setOrClearBit;
                }

                _storageAccessorForStates[MessageRedisplayBitMask] = bitMask;
            }
        }

        private void Subscribe()
        {
            _eventBus.Subscribe<OpenEvent>(this, HandleEvent);
            _eventBus.Subscribe<ClosedEvent>(this, HandleEvent);
            _eventBus.Subscribe<MessageRemovedEvent>(this, HandleEvent);
            _eventBus.Subscribe<OffEvent>(this, HandleEvent);
            _eventBus.Subscribe<OperatorMenuEnteredEvent>(
                this,
                _ =>
                {
                    _auditMenuWindowOpen = true;
                    if (_doorOpenAlarmOperatorCanCancel)
                    {
                        StopOpenDoorAlarm();
                    }
                });
            _eventBus.Subscribe<OperatorMenuExitingEvent>(
                this,
                _ =>
                {
                    _auditMenuWindowOpen = false;
                    if (!_doorOpenAlarmOperatorCanCancel)
                    {
                        CheckDoorAlarm(true, false);
                    }
                });
            _eventBus.Subscribe<MaintenanceModeEnteredEvent>(
                this,
                _ =>
                {
                    _maintenanceModeActive = true;
                    StopOpenDoorAlarm();
                });
            _eventBus.Subscribe<MaintenanceModeExitedEvent>(
                this,
                _ =>
                {
                    _maintenanceModeActive = false;
                    CheckDoorAlarm(true, false);
                });
        }

        private void SyncLifetimeMeter(int logicalId, bool whilePoweredDown)
        {
            var meterManager = ServiceManager.GetInstance().GetService<IMeterManager>();
            var door = _doors[logicalId];

            var meter = whilePoweredDown ? door.PowerOffMeterName : door.DoorMeterName;

            if (meterManager.IsMeterProvided(meter))
            {
                _storageAccessorForStates[meter] = meterManager.GetMeter(meter).Lifetime;
                Logger.Debug($"Lifetime meter {meter} synced {_storageAccessorForStates[meter]}");
            }
            else
            {
                Logger.Warn($"Attempt to sync unprovided door meter {meter}");
            }
        }
    }
}