namespace Aristocrat.Monaco.Hardware.Services
{
    using Contracts;
    using Contracts.Door;
    using Contracts.IO;
    using Contracts.Persistence;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using DisabledEvent = Contracts.Door.DisabledEvent;
    using EnabledEvent = Contracts.Door.EnabledEvent;

    /// <summary>Provides access to door services.</summary>
    public class DoorService : IDeviceService, IDoorService, IDisposable
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IIO _io;
        private readonly IPersistentStorageManager _storageManager;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IEventBus _bus;

        private readonly ConcurrentDictionary<int, LogicalDoor> _logicalDoors =
            new ConcurrentDictionary<int, LogicalDoor>();

        private bool _disposed;
        private DateTime _lastInput;
        private bool _platformBooted;
        private bool _isInspection;

        public DoorService()
            : this(
                ServiceManager.GetInstance().GetService<IIO>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        public DoorService(
            IIO io,
            IPersistentStorageManager storageManager,
            IEventBus bus,
            IPropertiesManager propertiesManager)
        {
            _io = io ?? throw new ArgumentNullException(nameof(io));
            _storageManager = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));

            Enabled = false;
        }

        private static DoorLogicalState LastEnabledLogicalState { get; set; }

        /// <inheritdoc />
        public bool Enabled { get; private set; }

        /// <inheritdoc />
        public bool Initialized => true;

        /// <inheritdoc />
        public string LastError { get; private set; }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public DisabledReasons ReasonDisabled { get; private set; }

        /// <inheritdoc />
        public string ServiceProtocol
        {
            get => string.Empty;
            set { }
        }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public void Disable(DisabledReasons reason)
        {
            Logger.Debug($"{Name} disabled by {reason}");
            ReasonDisabled |= reason;
            Enabled = false;
            if (LogicalState != DoorLogicalState.Disabled)
            {
                Logger.Debug($"Last enabled logical state {LastEnabledLogicalState} set to {LogicalState}");
                LastEnabledLogicalState = LogicalState;
            }

            LogicalState = DoorLogicalState.Disabled;
            _bus.Publish(new DisabledEvent(ReasonDisabled));
        }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public bool Enable(EnabledReasons reason)
        {
            if (Enabled)
            {
                Logger.Debug($"{Name} enabled by {reason} logical state {LogicalState}");
                _bus.Publish(new EnabledEvent(reason));
            }
            else if (Initialized)
            {
                if (((ReasonDisabled & DisabledReasons.Error) > 0 ||
                     (ReasonDisabled & DisabledReasons.FirmwareUpdate) > 0) &&
                    (reason == EnabledReasons.Reset || reason == EnabledReasons.Operator))
                {
                    if ((ReasonDisabled & DisabledReasons.Error) > 0)
                    {
                        Logger.Debug("Removed disabled reason " + DisabledReasons.Error);
                        ReasonDisabled &= ~DisabledReasons.Error;
                    }

                    if ((ReasonDisabled & DisabledReasons.FirmwareUpdate) > 0)
                    {
                        Logger.Debug("Removed disabled reason " + DisabledReasons.FirmwareUpdate);
                        ReasonDisabled &= ~DisabledReasons.FirmwareUpdate;
                    }
                }
                else if ((ReasonDisabled & DisabledReasons.Operator) > 0 && reason == EnabledReasons.Operator)
                {
                    Logger.Debug("Removed disabled reason " + DisabledReasons.Operator);
                    ReasonDisabled &= ~DisabledReasons.Operator;
                }
                else if ((ReasonDisabled & DisabledReasons.Service) > 0 && reason == EnabledReasons.Service)
                {
                    Logger.Debug("Removed disabled reason " + DisabledReasons.Service);
                    ReasonDisabled &= ~DisabledReasons.Service;
                }
                else if ((ReasonDisabled & DisabledReasons.System) > 0 && reason == EnabledReasons.System)
                {
                    Logger.Debug("Removed disabled reason " + DisabledReasons.System);
                    ReasonDisabled &= ~DisabledReasons.System;
                }
                else if ((ReasonDisabled & DisabledReasons.Configuration) > 0 && reason == EnabledReasons.Configuration)
                {
                    Logger.Debug("Removed disabled reason " + DisabledReasons.Configuration);
                    ReasonDisabled &= ~DisabledReasons.Configuration;
                }

                // Set enabled if we no longer have any disabled reasons.
                Enabled = ReasonDisabled == 0;
                if (Enabled)
                {
                    Logger.Debug($"{Name} enabled by {reason} logical state {LogicalState}");
                    if (LastEnabledLogicalState != DoorLogicalState.Uninitialized &&
                        LastEnabledLogicalState != DoorLogicalState.Idle)
                    {
                        Logger.Debug($"Logical state {LogicalState} reset to {DoorLogicalState.Idle}");
                        LogicalState = DoorLogicalState.Idle;
                    }
                    else
                    {
                        Logger.Debug($"Logical state {LogicalState} reset to {LastEnabledLogicalState}");
                        LogicalState = LastEnabledLogicalState;
                    }

                    _bus.Publish(new EnabledEvent(reason));
                }
                else
                {
                    Logger.Debug($"{Name} can not be enabled by {reason} because disabled by {ReasonDisabled}");
                    _bus.Publish(new DisabledEvent(ReasonDisabled));
                }
            }
            else
            {
                Logger.Debug($"{Name} can not be enabled by {reason} because service is not initialized");
                _bus.Publish(new DisabledEvent(ReasonDisabled));
            }

            return Enabled;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<int, LogicalDoor> LogicalDoors =>
            _logicalDoors
            .Where(logicalDoor => !IgnoredDoors.Contains(logicalDoor.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        public List<int> IgnoredDoors { get; set; } = new List<int>();

        /// <inheritdoc />
        public DoorLogicalState LogicalState { get; private set; }

        /// <inheritdoc />
        public string GetDoorName(int doorId)
        {
            return _logicalDoors.TryGetValue(doorId, out var door) ? door.Name : string.Empty;
        }

        public DateTime GetDoorLastOpened(int doorId)
        {
            return _logicalDoors.TryGetValue(doorId, out var door) ? door.LastOpenedDateTime : DateTime.MinValue;
        }

        /// <inheritdoc />
        public int GetDoorPhysicalId(int doorId)
        {
            return _logicalDoors.TryGetValue(doorId, out var door) ? door.PhysicalId : -1;
        }

        /// <inheritdoc />
        public DoorState GetDoorState(int doorId)
        {
            return _logicalDoors.TryGetValue(doorId, out var door) ? door.State : DoorState.Uninitialized;
        }

        /// <inheritdoc />
        public bool GetDoorClosed(int doorId) => !_logicalDoors.TryGetValue(doorId, out var door) || door.Closed;

        public bool GetDoorOpen(int doorId) => !GetDoorClosed(doorId);

        public string Name => "Door Service";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IDoorService) };

        public void Initialize()
        {
            LastError = string.Empty;

            _isInspection = (bool)_propertiesManager.GetProperty(Kernel.Contracts.KernelConstants.IsInspectionOnly, false);

            _bus.Subscribe<InputEvent>(this, HandleEvent);
            _bus.Subscribe<PlatformBootedEvent>(this, HandleEvent);

            var accessor = _storageManager.GetAccessor(Level, GetType().ToString());

            var config = _io.GetConfiguration();

            foreach (var item in config.Doors)
            {
                var door = new LogicalDoor(item.PhysicalId, item.Name, item.Name);

                if (_logicalDoors.TryAdd(item.LogicalId, door))
                {
                    door.State = DoorState.Enabled;

                    door.LastOpenedDateTime = (DateTime)accessor[GetLastDoorOpenStorageKey(item.LogicalId)];

                    Logger.Debug($"Adding logical Door {item.LogicalId} {door.Name} with physical ID {door.PhysicalId}");
                }
            }

            var queuedEvents = _io.GetQueuedEvents.OfType<InputEvent>().ToList();

            Logger.Debug($"Found {queuedEvents.Count} queued events");

            foreach (var evt in queuedEvents)
            {
                Logger.Debug($"Processing queued input event - {evt.GetType().Name}");
                _lastInput = evt.Timestamp;

                var door = _logicalDoors.FirstOrDefault(i => i.Value.PhysicalId == evt.Id);
                if (door.Value == null)
                {
                    continue;
                }

                door.Value.Closed = !evt.Action;
            }

            if (LogicalState == DoorLogicalState.Uninitialized)
            {
                LogicalState = DoorLogicalState.Idle;
            }

            Enable(EnabledReasons.Service);

            Logger.Info($"{Name} initialized");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _bus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private static string GetLastDoorOpenStorageKey(int logicalDoorId) => $"LogicalId{logicalDoorId}LastDoorOpen";

        private static string GetLastDoorOpenActionStorageKey(int logicalDoorId) => $"LogicalId{logicalDoorId}LastDoorOpenAction";

        private void HandleEvent(InputEvent theEvent)
        {
            HandleEvent(theEvent, false);
        }

        private void HandleEvent(PlatformBootedEvent theEvent)
        {
            _bus.Unsubscribe<PlatformBootedEvent>(this);

            var queuedEvents = _io.GetQueuedEvents.OfType<InputEvent>().ToList();
            Logger.DebugFormat($"Found {queuedEvents.Count} queued events");

            // if no new input events have been received since HardwareDiscoveryCompleted
            // then process all to make sure no one missed the initial state
            if (queuedEvents.Count > 0 && queuedEvents.LastOrDefault()?.Timestamp == _lastInput)
            {
                _lastInput = DateTime.MinValue;
            }

            foreach (var nextEvent in queuedEvents)
            {
                Logger.DebugFormat($"Processing queued input event - {nextEvent.GetType().Name}");
                HandleEvent(nextEvent, true);
            }

            _platformBooted = true;
        }

        private void HandleEvent(InputEvent inEvent, bool whilePoweredDown)
        {
            if (inEvent.Timestamp >= _lastInput || _platformBooted || _isInspection)
            {
                _lastInput = inEvent.Timestamp;

                var items = LogicalDoors.Where(i => i.Value.PhysicalId == inEvent.Id);
                foreach (var item in items)
                {
                    var doorId = item.Key;
                    var door = item.Value;

                    if (door != null)
                    {
                        door.Closed = !inEvent.Action;
                        var accessor = ServiceManager.GetInstance().GetService<IPersistentStorageManager>()
                            .GetAccessor(Level, GetType().ToString());

                        if (door.State == DoorState.Enabled)
                        {
                            var closed = true;
                            if (accessor[GetLastDoorOpenActionStorageKey(doorId)] != null)
                            {
                                closed = !(bool)accessor[GetLastDoorOpenActionStorageKey(doorId)];
                            }

                            if (inEvent.Action)
                            {
                                // only set date if the last action does not match current action or the game is first setting
                                // up and no date is set. This prevents the date from being set if the door is open during boot
                                // up if it was opened when shutting game down.
                                if (closed != door.Closed || door.LastOpenedDateTime == DateTime.MinValue)
                                {
                                    accessor[GetLastDoorOpenStorageKey(doorId)] =
                                        door.LastOpenedDateTime = DateTime.UtcNow;
                                }

                                _bus.Publish(new OpenEvent(doorId, whilePoweredDown, GetDoorName(doorId)));
                            }
                            else
                            {
                                _bus.Publish(new ClosedEvent(doorId, whilePoweredDown, GetDoorName(doorId)));
                            }

                            accessor[GetLastDoorOpenActionStorageKey(doorId)] = door.Closed == false;

                            Logger.Debug(
                                $"Physical Door {inEvent.Id} closed:{door.Closed}, logical Door {doorId} {door.Name} closed:{door.Closed} event posted");
                        }
                        else
                        {
                            Logger.Debug(
                                $"Physical Door {inEvent.Id} closed {door.Closed}, logical Door {doorId} {door.Name} disabled");
                        }
                    }
                    else
                    {
                        Logger.Warn($"Logical Door {doorId} for physical IO {inEvent.Id} missing from dictionary");
                    }
                }
            }
        }
    }
}