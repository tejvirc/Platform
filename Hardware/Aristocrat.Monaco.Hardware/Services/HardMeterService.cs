namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Contracts;
    using Contracts.Door;
    using Contracts.HardMeter;
    using Contracts.IO;
    using Contracts.KeySwitch;
    using Contracts.Persistence;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;
    using DisabledEvent = Contracts.HardMeter.DisabledEvent;
    using EnabledEvent = Contracts.HardMeter.EnabledEvent;
    using OffEvent = Contracts.KeySwitch.OffEvent;
    using OnEvent = Contracts.KeySwitch.OnEvent;

    /// <summary>
    ///     Provides implementation of HardMeterService. This component handles mapping and handling of physical input events
    ///     from IO services and posting the associated logical events to the system. Also provides and interface for handling
    ///     the device from operator menu.
    /// </summary>
    public class HardMeterService : BaseRunnable, IHardMeter
    {
        private const int AllMeters = 0x3F; // 0011 1111 (meters 0-5)
        private const string HardMeterLightSwitch = "Hard Meter Light";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string BlockDataMeterValue = "MeterValue";
        private static readonly TimeSpan MonitorInterval = TimeSpan.FromSeconds(5);
        private readonly IEventBus _bus;
        private readonly IIO _io;
        private readonly IKeySwitch _keySwitch;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IPropertiesManager _properties;
        private readonly string _blockDataTickValue = "TickValue";
        private readonly IOConfigurations _ioConfiguration;
        private readonly AutoResetEvent _meterRequestEvent = new AutoResetEvent(false);
        private readonly object _lock = new object();
        private readonly string _blockName = typeof(HardMeterService).ToString();
        private readonly string _pendingBlockName = $"{typeof(HardMeterService)}Pending";
        private readonly Dictionary<string, IPersistentStorageAccessor> _accessors = new Dictionary<string, IPersistentStorageAccessor>();
        private readonly Dictionary<int, long> _pendingMeters = new Dictionary<int, long>();
        private Timer _monitor;
        private bool _hardMeterEnabled;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HardMeterService" /> class.
        /// </summary>
        public HardMeterService()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IIO>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IKeySwitch>())
        {
        }

        public HardMeterService(
            IEventBus bus,
            IIO io,
            IPersistentStorageManager persistentStorage,
            IPropertiesManager properties,
            IKeySwitch keySwitch)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _io = io ?? throw new ArgumentNullException(nameof(io));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _keySwitch = keySwitch ?? throw new ArgumentNullException(nameof(keySwitch));

            Disable(DisabledReasons.Service);
            _ioConfiguration = _io.GetConfiguration();
            InitializeBlocks();
        }

        /// <inheritdoc />
        public bool Enabled { get; private set; }

        /// <inheritdoc />
        public bool Initialized => RunState != RunnableState.Uninitialized;

        /// <inheritdoc />
        public string LastError { get; private set; }

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
            Logger.Debug(Name + " disabled by " + reason);
            ReasonDisabled |= reason;

            Enabled = false;
            LogicalState = HardMeterLogicalState.Disabled;

            if (_hardMeterEnabled)
            {
                _bus?.Publish(new DisabledEvent(ReasonDisabled));
            }
        }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public bool Enable(EnabledReasons reason)
        {
            if (!_hardMeterEnabled)
            {
                return false;
            }

            if (Enabled)
            {
                Logger.Debug(Name + " enabled by " + reason + " logical state " + LogicalState);
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
                else if ((ReasonDisabled & DisabledReasons.Configuration) > 0 &&
                         reason == EnabledReasons.Configuration)
                {
                    Logger.Debug("Removed disabled reason " + DisabledReasons.Configuration);
                    ReasonDisabled &= ~DisabledReasons.Configuration;
                }

                // Set enabled if we no longer have any disabled reasons.
                Enabled = ReasonDisabled == 0;
                if (Enabled)
                {
                    Logger.Debug(Name + " enabled by " + reason + " logical state " + LogicalState);

                    LogicalState = HardMeterLogicalState.Idle;

                    _bus.Publish(new EnabledEvent(reason));
                }
                else
                {
                    Logger.Warn(Name + " can not be enabled by " + reason + " because disabled by " + ReasonDisabled);
                    _bus.Publish(new DisabledEvent(ReasonDisabled));
                }
            }
            else
            {
                Logger.Warn(Name + " can not be enabled by " + reason + " because service is not initialized");
                _bus.Publish(new DisabledEvent(ReasonDisabled));
            }

            _meterRequestEvent.Set();

            return Enabled;
        }

        /// <inheritdoc />
        public Dictionary<int, LogicalHardMeter> LogicalHardMeters { get; } = new Dictionary<int, LogicalHardMeter>();

        /// <inheritdoc />
        public HardMeterLogicalState LogicalState { get; set; }

        /// <inheritdoc />
        public void AdvanceHardMeter(int hardMeterId, long value)
        {
            if (LogicalHardMeters.TryGetValue(hardMeterId, out var hardMeter))
            {
                if (!_accessors.TryGetValue(_pendingBlockName, out var accessor))
                {
                    Logger.Error("HardMeterService AdvanceHardMeter was unable to access persistence");
                    return;
                }

                lock (_lock)
                {
                    using (var transaction = accessor.StartTransaction())
                    {
                        var current = (long)transaction[hardMeter.LogicalId, BlockDataMeterValue];

                        transaction[hardMeter.LogicalId, BlockDataMeterValue] = current + value;
                        transaction.Commit();

                        if (PersistenceTransaction.Current == null)
                        {
                            _meterRequestEvent.Set();
                        }
                        else
                        {
                            transaction.OnCompleted += OnCompleteScopedTransaction;
                        }

                        Logger.Debug(
                            $"Advanced HardMeter {hardMeterId} {hardMeter.Name} {hardMeter.State} from {current} to {current + value}");
                    }
                }
            }
            else
            {
                Logger.Warn($"Cannot advance logical HardMeter {hardMeterId}, need to add it first");
            }
        }

        /// <inheritdoc />
        public HardMeterAction GetHardMeterAction(int hardMeterId)
        {
            return !LogicalHardMeters.TryGetValue(hardMeterId, out var hardMeter)
                ? HardMeterAction.Off
                : hardMeter.Action;
        }

        /// <inheritdoc />
        public string GetHardMeterName(int hardMeterId)
        {
            return !LogicalHardMeters.TryGetValue(hardMeterId, out var hardMeter) ? string.Empty : hardMeter.Name;
        }

        /// <inheritdoc />
        public int GetHardMeterPhysicalId(int hardMeterId)
        {
            return !LogicalHardMeters.TryGetValue(hardMeterId, out var hardMeter) ? -1 : hardMeter.PhysicalId;
        }

        /// <inheritdoc />
        public HardMeterState GetHardMeterState(int hardMeterId)
        {
            return !LogicalHardMeters.TryGetValue(hardMeterId, out var hardMeter)
                ? HardMeterState.Uninitialized
                : hardMeter.State;
        }

        /// <inheritdoc />
        public string GetLocalizedHardMeterName(int hardMeterId)
        {
            return !LogicalHardMeters.TryGetValue(hardMeterId, out var hardMeter)
                ? string.Empty
                : hardMeter.LocalizedName;
        }

        public void UpdateTickValues(Dictionary<int, long> listOfMeters)
        {
            lock (_lock)
            {
                foreach (var logicalHardMeter in LogicalHardMeters.Values.Where(
                    l => listOfMeters.ContainsKey(l.LogicalId)))
                {
                    logicalHardMeter.TickValue = listOfMeters[logicalHardMeter.LogicalId];
                    _accessors[_blockName][logicalHardMeter.LogicalId, _blockDataTickValue] =
                        listOfMeters[logicalHardMeter.LogicalId];
                }
            }
        }

        public long GetHardMeterValue(int hardMeterId)
        {
            if (GetHardMeterPhysicalId(hardMeterId) == -1)
            {
                return 0;
            }

            lock (_lock)
            {
                return (long)_accessors[_blockName][hardMeterId, BlockDataMeterValue];
            }
        }

        public bool IsHardwareOperational => !CheckStoppedResponding(_io, AllMeters);

        /// <inheritdoc />
        public string Name => typeof(HardMeterService).ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IHardMeter) };

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            LastError = string.Empty;

            _hardMeterEnabled = _properties.GetValue(HardwareConstants.HardMetersEnabledKey, false);

            if (!_hardMeterEnabled)
            {
                return;
            }

            _bus.Subscribe<OutputEvent>(this, HandleEvent);
            _bus.Subscribe<ErrorEvent>(this, HandleEvent);
            _bus.Subscribe<PersistentStorageClearStartedEvent>(this, _ => Stop());
            _bus.Subscribe<OnEvent>(
                this,
                UpdateHardMeterLight,
                e => e.LogicalId == _keySwitch.GetKeySwitchId(HardMeterLightSwitch));
            _bus.Subscribe<OffEvent>(
                this,
                UpdateHardMeterLight,
                e => e.LogicalId == _keySwitch.GetKeySwitchId(HardMeterLightSwitch));
            _bus.Subscribe<OpenEvent>(this, UpdateHardMeterLight, e => e.LogicalId == (int)DoorLogicalId.TopBox || e.LogicalId == (int)DoorLogicalId.UniversalInterfaceBox);
            _bus.Subscribe<ClosedEvent>(this, UpdateHardMeterLight, e => e.LogicalId == (int)DoorLogicalId.TopBox || e.LogicalId == (int)DoorLogicalId.UniversalInterfaceBox);
            _bus.Subscribe<PropertyChangedEvent>(
                this,
                HandleEvent,
                @event => @event.PropertyName.Equals(HardwareConstants.HardMetersEnabledKey));

            lock (_lock)
            {
                foreach (var hardMeter in _ioConfiguration.HardMeters.HardMeter)
                {
                    LogicalHardMeters.Add(
                        hardMeter.LogicalId,
                        new LogicalHardMeter(
                            hardMeter.PhysicalId,
                            hardMeter.LogicalId,
                            hardMeter.Name,
                            hardMeter.Name,
                            (long)_accessors[_blockName][hardMeter.LogicalId, _blockDataTickValue]));

                    LogicalHardMeters[hardMeter.LogicalId].State =
                        hardMeter.Enabled ? HardMeterState.Enabled : HardMeterState.Disabled;
                }
            }

            Enable(EnabledReasons.Service);

            CheckStoppedResponding(_io, AllMeters);

            _monitor = new Timer(OnHealthCheck, null, MonitorInterval, System.Threading.Timeout.InfiniteTimeSpan);

            Logger.Info(Name + " initialized");
        }

        /// <inheritdoc />
        protected override void OnRun()
        {
            Logger.Info(Name + " started");

            if (!_hardMeterEnabled)
            {
                return;
            }

            var pending = PendingIncrement();

            while (RunState == RunnableState.Running)
            {
                if (RunState != RunnableState.Running || !Enabled ||
                    !LogicalHardMeters.Any(m => m.Value.State == HardMeterState.Enabled && !m.Value.Suspended))
                {
                    _meterRequestEvent.WaitOne();

                    continue;
                }

                IncrementHardMeter(pending);

                pending = PendingIncrement(pending);
                if (pending.Count > 0)
                {
                    continue;
                }

                _meterRequestEvent.WaitOne();

                pending = PendingIncrement();
            }
        }

        /// <inheritdoc />
        protected override void OnStop()
        {
            Logger.Info(Name + " stopping");

            _monitor?.Change(System.Threading.Timeout.InfiniteTimeSpan, System.Threading.Timeout.InfiniteTimeSpan);

            _bus.UnsubscribeAll(this);

            Disable(DisabledReasons.Service);

            // Set meter request event to unblock the runnable in order to stop.
            _meterRequestEvent.Set();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            base.Dispose(disposing);

            if (disposing)
            {
                _meterRequestEvent.Close();

                if (_monitor != null)
                {
                    _monitor.Change(
                        System.Threading.Timeout.InfiniteTimeSpan,
                        System.Threading.Timeout.InfiniteTimeSpan);
                    _monitor.Dispose();
                    _monitor = null;
                }
            }
        }

        private static void RollbackTick(IPersistentStorageAccessor block, LogicalHardMeter hardMeter)
        {
            using (var transaction = block.StartTransaction())
            {
                transaction[hardMeter.LogicalId, BlockDataMeterValue] = hardMeter.Count;
                transaction.Commit();
            }
        }

        private void HandleEvent(PropertyChangedEvent evt)
        {
            _hardMeterEnabled = _properties.GetValue(HardwareConstants.HardMetersEnabledKey, false);

            if (!_hardMeterEnabled && (ReasonDisabled & DisabledReasons.Error) != 0)
            {
                _bus.Publish(new EnabledEvent(EnabledReasons.Service));
                Logger.Debug("Removed disabled reason " + DisabledReasons.Error);
                ReasonDisabled &= ~DisabledReasons.Error;
                Disable(DisabledReasons.Service);
            }
        }

        private void OnHealthCheck(object state)
        {
            CheckStoppedResponding(_io, AllMeters);

            try
            {
                _monitor?.Change(MonitorInterval, System.Threading.Timeout.InfiniteTimeSpan);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private IReadOnlyCollection<LogicalHardMeter> PendingIncrement(IReadOnlyCollection<LogicalHardMeter> incrementedMeters=null)
        {
            var pending = new List<LogicalHardMeter>();

            lock (_lock)
            {
                var block = _accessors[_blockName];
                var blockPending = _accessors[_pendingBlockName];

                foreach (var meter in LogicalHardMeters
                                          .Where(meter => (long)blockPending[meter.Value.LogicalId, BlockDataMeterValue] > 0))
                {
                    _pendingMeters[meter.Value.LogicalId] = (long)blockPending[meter.Value.LogicalId, BlockDataMeterValue];

                    Logger.Info($"Updating pending meter {meter.Value.Name} {_pendingMeters[meter.Value.LogicalId]}");
                }
                
                if (_pendingMeters.Any())
                {
                    using (var scope = _persistentStorage.ScopedTransaction())
                    {
                        using (var transaction = blockPending.StartTransaction())
                        {
                            foreach (var meter in _pendingMeters)
                            {
                                transaction[meter.Key, BlockDataMeterValue] = 0L;
                            }

                            transaction.Commit();
                        }

                        using (var transaction = block.StartTransaction())
                        {
                            foreach (var meter in LogicalHardMeters)
                            {
                                var current = (long)transaction[meter.Value.LogicalId, BlockDataMeterValue]
                                              + (_pendingMeters.ContainsKey(meter.Value.LogicalId)
                                                  ? _pendingMeters[meter.Value.LogicalId]
                                                  : 0);

                                AddPending(meter, current);

                                transaction[meter.Value.LogicalId, BlockDataMeterValue] = current;
                            }

                            transaction.Commit();
                        }

                        scope.Complete();
                    }

                    _pendingMeters.Clear();
                }
                else
                {
                    foreach (var meter in LogicalHardMeters)
                    {
                        AddPending(meter, (long)block[meter.Value.LogicalId, BlockDataMeterValue]);
                    }
                }

                // Find the completed meters and publish the events
                if (incrementedMeters != null)
                {
                    // find the meters not in the pending meters from last incremented meters, then those are completed
                    var completedMeters = incrementedMeters.Except(pending);
                    foreach (var meter in completedMeters)
                    {
                        _bus.Publish(new HardMeterTickStoppedEvent(meter.LogicalId));
                    }
                }

                return pending;

                void AddPending(KeyValuePair<int, LogicalHardMeter> meter, long current)
                {
                    meter.Value.Count = current;
                    if (meter.Value.Count >= meter.Value.TickValue)
                    {
                        pending.Add(meter.Value);
                    }
                }
            }
        }

        private void IncrementHardMeter(IReadOnlyCollection<LogicalHardMeter> meters)
        {
            if (CheckStoppedResponding(_io, AllMeters))
            {
                return;
            }

            foreach (var meter in meters)
            {
                lock (_lock)
                {
                    if (!TickMeter(meter, _io))
                    {
                        break;
                    }
                }
            }
        }

        private bool TickMeter(LogicalHardMeter hardMeter, IIO io)
        {
            // Set the bit for this hard meters physical Id.
            // *NOTE* We only start a transaction for a single hard meter to prevent problems when rolling back
            // a transaction if the increment fails due to a disconnect.  Depending on the timing of the disconnect, when
            // incrementing more than one meter (more than one bit set in the mask) it has been observed that some of
            // target meters may increment but other meters do not.  To prevent the transaction logic from rolling back
            // the meters that successfully incremented, we need to use a single transaction for a single meter increment.
            // DO NOT SET MORE THAN ONE BIT (ONE METER) PER TRANSACTION.
            var meterIdMask = 0;
            meterIdMask |= 1 << hardMeter.PhysicalId;

            using (var transaction = _accessors[_blockName].StartTransaction())
            {
                var current = (long)transaction[hardMeter.LogicalId, BlockDataMeterValue];

                hardMeter.Count = current - hardMeter.TickValue;

                // Check mechanical Voltage current high (cleared)
                var status = io.StatusMechanicalMeter(meterIdMask);

                // Set voltage current high (clear)
                if (status != meterIdMask)
                {
                    Logger.Error(
                        $"Hard meter {hardMeter.Name} stopped responding during increment with status {status}");

                    SetStoppedResponding(true);

                    return false;
                }

                transaction[hardMeter.LogicalId, BlockDataMeterValue] = hardMeter.Count;
                transaction.Commit();

                // Check mechanical Voltage current low (set), We have a hardware commit from here and on. (if anything crashes beyond this point, the meter has been increment)
                io.SetMechanicalMeter(meterIdMask);

                status = io.StatusMechanicalMeter(AllMeters & ~meterIdMask);

                // set current high (clear)
                io.ClearMechanicalMeter(meterIdMask);

                // Check that the voltage current is low (all id's should be set).
                if (status != (AllMeters & ~meterIdMask))
                {
                    Logger.Error(
                        $"Hard meter {hardMeter.Name} stopped responding during increment with status {status} - rolling back from {hardMeter.Count} to {current}");

                    // Rollback the the tick since the mechanical meter wasn't updated
                    hardMeter.Count = current;
                    RollbackTick(_accessors[_blockName], hardMeter);

                    SetStoppedResponding(true);

                    return false;
                }
            }

            return true;
        }

        private void InitializeBlocks()
        {
            InitializeBlock();
            InitializePendingBlock();
        }
        
        private void InitializeBlock()
        {
            IPersistentStorageAccessor accessor;

            if (_persistentStorage.BlockExists(_blockName))
            {
                accessor = _persistentStorage.GetBlock(_blockName);
            }
            else
            {
                accessor = _persistentStorage.CreateBlock(
                    PersistenceLevel.Critical,
                    _blockName,
                    _ioConfiguration.HardMeters.HardMeter.Max(n => n.LogicalId) + 1);
                // init for the first time only
                using (var transaction = accessor.StartTransaction())
                {
                    foreach (var ioConfigurationsHardMeter in _ioConfiguration.HardMeters.HardMeter)
                    {
                        transaction[ioConfigurationsHardMeter.LogicalId, "TickValue"] =
                            _ioConfiguration.HardMeters.DefaultTickValue;
                    }

                    transaction.Commit();
                }
            }

            _accessors[_blockName] = accessor;
        }

        private void InitializePendingBlock()
        {
            if (_accessors.ContainsKey(_pendingBlockName))
            {
                return;
            }

            IPersistentStorageAccessor accessor;

            if (_persistentStorage.BlockExists(_pendingBlockName))
            {
                accessor = _persistentStorage.GetBlock(_pendingBlockName);
            }
            else
            {
                accessor = _persistentStorage.CreateBlock(
                    PersistenceLevel.Critical,
                    _pendingBlockName,
                    _ioConfiguration.HardMeters.HardMeter.Max(n => n.LogicalId) + 1);
            }

            _accessors[_pendingBlockName] = accessor;            
        }

        private bool CheckStoppedResponding(IIO io, int bitField)
        {
            var stopped = false;
            int status;

            lock (_lock)
            {
                status = io.StatusMechanicalMeter(bitField);
            }

            if (status == bitField)
            {
                if ((ReasonDisabled & DisabledReasons.Error) == DisabledReasons.Error)
                {
                    Logger.Debug($"Hard meters {bitField} started responding");
                    SetStoppedResponding(false);
                }
            }
            else
            {
                if ((ReasonDisabled & DisabledReasons.Error) != DisabledReasons.Error)
                {
                    Logger.Error($"Hard meters {bitField} stopped responding with {status}");
                    SetStoppedResponding(true);
                }

                stopped = true;
            }

            return stopped;
        }

        private void SetStoppedResponding(bool stopped)
        {
            Logger.Debug($"SetStoppedResponding: {stopped}");

            if (stopped)
            {
                Disable(DisabledReasons.Error);

                _bus.Publish(new StoppedRespondingEvent());
            }
            else
            {
                Enable(EnabledReasons.Reset);

                _bus.Publish(new StartedRespondingEvent());
            }
        }

        private void HandleEvent(ErrorEvent evt)
        {
            var id = evt.Id;

            switch (id)
            {
                case ErrorEventId.InvalidHandle:
                case ErrorEventId.ReadBoardInfoFailure:
                {
                    // TODO: the level should be "WARN" before the error occurrence threshold is hit.
                    // And log an error when the threshold is hit.
                    Logger.Error($"Handled error {id}");

                    // Disable service for error.
                    Disable(DisabledReasons.Error);
                    break;
                }

                default:
                    Logger.Warn($"Unhandled ErrorEventId {id}");
                    break;
            }
        }

        private void HandleEvent(OutputEvent evt)
        {
            var meter = LogicalHardMeters.FirstOrDefault(m => m.Value.PhysicalId == evt.Id);

            var hardMeterId = meter.Key;
            var hardMeter = meter.Value;

            if (hardMeter == null)
            {
                return;
            }

            hardMeter.Action = evt.Action ? HardMeterAction.Off : HardMeterAction.On;

            if (hardMeter.State == HardMeterState.Enabled)
            {
                if (evt.Action)
                {
                    _bus.Publish(new Contracts.HardMeter.OnEvent(hardMeterId));
                }
                else
                {
                    _bus.Publish(new Contracts.HardMeter.OffEvent(hardMeterId));
                }

                Logger.Debug(
                    $"Physical HardMeter {evt.Id} {hardMeter.Action}, logical HardMeter {hardMeterId} {hardMeter.Name} {hardMeter.Action} event posted");
            }
            else
            {
                Logger.Debug(
                    $"Physical HardMeter {evt.Id} {hardMeter.Action}, logical HardMeter {hardMeterId} {hardMeter.Name} disabled");
            }
        }

        private void OnCompleteScopedTransaction(object sender, TransactionEventArgs scopedTransactionEventArgs)
        {
            if (scopedTransactionEventArgs.Committed)
            {
                _meterRequestEvent.Set();
            }

            if (sender is IPersistentStorageTransaction transaction)
            {
                transaction.OnCompleted -= OnCompleteScopedTransaction;
            }
        }

        private void UpdateHardMeterLight(IEvent e)
        {
            _io.SetMechanicalMeterLight(
                (ServiceManager.GetInstance().TryGetService<IDoorService>()?.GetDoorOpen((int)DoorLogicalId.TopBox) ??
                 false)
                || _keySwitch.GetKeySwitchAction(_keySwitch.GetKeySwitchId(HardMeterLightSwitch)) ==
                KeySwitchAction.On);
        }
    }
}