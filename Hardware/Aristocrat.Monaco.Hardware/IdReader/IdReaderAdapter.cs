namespace Aristocrat.Monaco.Hardware.IdReader
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using Contracts;
    using Contracts.CardReader;
    using Contracts.Communicator;
    using Contracts.IdReader;
    using Contracts.Persistence;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;
    using Stateless;
    using Timer = System.Timers.Timer;

    /// <summary>An ID reader adapter.</summary>
    /// <seealso
    ///     cref="T:Aristocrat.Monaco.Hardware.Contracts.SharedDevice.DeviceAdapterBase{Aristocrat.Monaco.Hardware.Contracts.IdReader.IIdReaderImplementation}" />
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.IdReader.IIdReader" />
    public class IdReaderAdapter : DeviceAdapter<IIdReaderImplementation>,
        IIdReader,
        IStorageAccessor<IdReaderOptions>
    {
        private const string DeviceImplementationsExtensionPath = "/Hardware/IdReader/IdReaderImplementations";

        private const string OptionsBlock = "Aristocrat.Monaco.Hardware.IdReader.IdReaderAdapter.Options";
        private const string IdReaderTypeOption = "IdReaderType";
        private const string IdReaderTrackOption = "IdReaderTrack";
        private const string ValidationMethodOption = "ValidationMethod";
        private const string WaitTimeoutOption = "WaitTimeout";
        private const string RemovalDelayOption = "RemovalDelayTimeout";
        private const string ValidationTimeoutOption = "ValidationTimeout";
        private const string SupportsOfflineValidationOption = "SupportsOfflineValidation";
        private const double IdleTimerInterval = 1000;
        private const int DefaultTimeout = 300000;
        public const int DefaultRemovalDelay = 5000;
        public const int DefaultWaitTimeout = 300000;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly StateMachine<IdReaderLogicalState, IdReaderLogicalStateTrigger> _state;

        private readonly ReaderWriterLockSlim _stateLock =
            new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private List<OfflineValidationPattern> _patterns = new List<OfflineValidationPattern>();

        private Identity _identity;
        private IIdReaderImplementation _reader;
        private bool _supportsOfflineValidation;
        private Timer _timer;
        private int _validationTimeout;
        private int _waitTimeout;
        private int _removalDelay;
        private bool _requiredForPlay;

        /// <summary>
        ///     Initializes a new instance of the Aristocrat.Monaco.Hardware.IdReader.IdReaderAdapter class.
        /// </summary>
        public IdReaderAdapter()
        {
            _state = ConfigureStateMachine();

            _timer = new Timer(IdleTimerInterval);

            _timer.Elapsed += CheckForTimeout;
        }

        /// <inheritdoc />
        protected override IIdReaderImplementation Implementation => _reader;

        /// <inheritdoc />
        protected override string Description => string.Empty;

        /// <inheritdoc />
        protected override string Path => string.Empty;

        private IUserActivityService _userActivityService =>
            ServiceManager.GetInstance().GetService<IUserActivityService>();

        private bool ShouldCheckForUserActivity => ValidationTimeoutTimeSpan > TimeSpan.Zero;

        private TimeSpan ValidationTimeoutTimeSpan => TimeSpan.FromMilliseconds(ValidationTimeout);

        /// <inheritdoc />
        public int IdReaderId { get; set; } = 1; // Default to deviceId 1 since 0 isn't valid in G2S

        /// <inheritdoc />
        public bool IsEgmControlled => Implementation?.IsEgmControlled ?? false;

        /// <inheritdoc />
        public bool IsImplementationEnabled => Implementation?.IsEnabled ?? false;

        public override DeviceType DeviceType => DeviceType.IdReader;

        /// <inheritdoc />
        public override bool Connected => !(_state?.State == IdReaderLogicalState.Uninitialized ||
                                            _state?.State == IdReaderLogicalState.Disconnected ||
                                            (!Implementation?.IsConnected ?? false));

        /// <inheritdoc />
        public bool RequiredForPlay
        {
            get => _requiredForPlay;
            set
            {
                _requiredForPlay = value;

                if (!_requiredForPlay)
                {
                    // Remove disabled message if RequiredForPlay changes from true to false
                    var bus = ServiceManager.GetInstance().GetService<IEventBus>();
                    bus.Publish(new RemoveDisabledMessageEvent(IdReaderId));
                }
                else if (LogicalState == IdReaderLogicalState.Disconnected)
                {
                    // Send DisconnectedEvent if RequiredForPlay changes from false to true and ID Reader is disconnected
                    var bus = ServiceManager.GetInstance().GetService<IEventBus>();
                    bus.Publish(new DisconnectedEvent(IdReaderId));
                }
            }
        }

        /// <inheritdoc />
        public IdReaderTypes IdReaderType
        {
            get => Implementation?.IdReaderType ?? IdReaderTypes.None;
            set
            {
                if (Implementation != null && Implementation.IdReaderType != value)
                {
                    Implementation.IdReaderType = value;
                    ModifyBlock(IdReaderTypeOption, _reader.IdReaderType);
                }
            }
        }

        /// <inheritdoc />
        public IdReaderTracks IdReaderTrack
        {
            get => Implementation?.IdReaderTrack ?? IdReaderTracks.None;
            set
            {
                if (Implementation != null && Implementation.IdReaderTrack != value)
                {
                    Implementation.IdReaderTrack = value;
                    ModifyBlock(IdReaderTrackOption, _reader.IdReaderTrack);
                }
            }
        }

        /// <inheritdoc />
        public IdValidationMethods ValidationMethod
        {
            get => Implementation?.ValidationMethod ?? IdValidationMethods.Host;
            set
            {
                if (Implementation != null && Implementation.ValidationMethod != value)
                {
                    Implementation.ValidationMethod = value;
                    ModifyBlock(ValidationMethodOption, (byte)_reader.ValidationMethod);
                }
            }
        }

        /// <inheritdoc />
        public int WaitTimeout
        {
            get => _waitTimeout;
            set
            {
                if (_waitTimeout != value)
                {
                    _waitTimeout = value;
                    ModifyBlock(WaitTimeoutOption, _waitTimeout);
                }
            }
        }

        /// <inheritdoc />
        public int RemovalDelay
        {
            get => _removalDelay;
            set
            {
                if (_removalDelay != value)
                {
                    _removalDelay = value;
                    ModifyBlock(RemovalDelayOption, _removalDelay);
                }
            }
        }

        /// <inheritdoc />
        public int ValidationTimeout
        {
            get => _validationTimeout;
            set
            {
                if (_validationTimeout != value)
                {
                    _validationTimeout = value;
                    ModifyBlock(ValidationTimeoutOption, _validationTimeout);
                }
            }
        }

        /// <inheritdoc />
        public bool SupportsOfflineValidation
        {
            get => _supportsOfflineValidation;

            set
            {
                if (_supportsOfflineValidation != value)
                {
                    _supportsOfflineValidation = value;
                    ModifyBlock(SupportsOfflineValidationOption, _supportsOfflineValidation);
                }
            }
        }

        /// <inheritdoc />
        public Identity Identity
        {
            get => _identity;

            private set
            {
                _identity = value;

                if (ShouldCheckForUserActivity)
                {
                    if (_identity == null)
                    {
                        _timer.Stop();
                    }
                    else
                    {
                        _timer.Start();
                    }
                }
            }
        }

        /// <inheritdoc />
        public string CardData => _reader?.TrackData?.IdNumber ?? string.Empty;

        /// <inheritdoc />
        public TrackData TrackData => _reader?.TrackData;

        /// <inheritdoc />
        public IEnumerable<OfflineValidationPattern> Patterns
        {
            get => _patterns;

            // TODO: This needs to be persisted
            set => _patterns = (value ?? Enumerable.Empty<OfflineValidationPattern>()).ToList();
        }

        /// <inheritdoc />
        public IdReaderLogicalState LogicalState => _state.State;

        /// <inheritdoc />
        public override string Name => string.IsNullOrEmpty(ServiceProtocol) == false
            ? $"{ServiceProtocol} ID Reader Adapter"
            : "Unknown ID Reader Adapter";

        /// <inheritdoc />
        public override ICollection<Type> ServiceTypes => new[] { typeof(IIdReader) };

        /// <inheritdoc />
        public DateTime TimeOfLastIdentityHandled { get; set; }

        /// <inheritdoc />
        public IdReaderFaultTypes Faults => Implementation?.Faults ?? IdReaderFaultTypes.None;

        /// <inheritdoc />
        public override async Task<bool> SelfTest(bool clear)
        {
            var result = await base.SelfTest(clear);
            if (result)
            {
                Logger.Debug("Self test passed");
                PostEvent(new SelfTestPassedEvent(IdReaderId));
            }
            else
            {
                Logger.Error("Self test failed");
                PostEvent(new SelfTestFailedEvent(IdReaderId));
            }

            return result;
        }

        /// <inheritdoc />
        public bool TryAddBlock(IPersistentStorageAccessor accessor, int blockIndex, out IdReaderOptions block)
        {
            // Set the Id reader adapter defaults
            block = new IdReaderOptions
            {
                IdReaderType = _reader.IdReaderType,
                IdReaderTrack =
                    _reader.IdReaderTrack != IdReaderTracks.None ? _reader.IdReaderTrack : IdReaderTracks.Track1,
                ValidationMethod = _reader.ValidationMethod,
                WaitTimeout = DefaultTimeout,
                ValidationTimeout = DefaultTimeout,
                RemovalDelay = DefaultRemovalDelay,
                SupportsOfflineValidation = false
            };

            using (var transaction = accessor.StartTransaction())
            {
                transaction[blockIndex, IdReaderTypeOption] = (int)block.IdReaderType;
                transaction[blockIndex, IdReaderTrackOption] = (int)block.IdReaderTrack;
                transaction[blockIndex, ValidationMethodOption] = (byte)block.ValidationMethod;
                transaction[blockIndex, WaitTimeoutOption] = block.WaitTimeout;
                transaction[blockIndex, RemovalDelayOption] = block.RemovalDelay;
                transaction[blockIndex, ValidationTimeoutOption] = block.ValidationTimeout;
                transaction[blockIndex, SupportsOfflineValidationOption] = block.SupportsOfflineValidation;

                transaction.Commit();
                return true;
            }
        }

        /// <inheritdoc />
        public bool TryGetBlock(IPersistentStorageAccessor accessor, int blockIndex, out IdReaderOptions block)
        {
            block = new IdReaderOptions
            {
                IdReaderType = (IdReaderTypes)(int)accessor[blockIndex, IdReaderTypeOption],
                IdReaderTrack = (IdReaderTracks)(int)accessor[blockIndex, IdReaderTrackOption],
                ValidationMethod = (IdValidationMethods)(byte)accessor[blockIndex, ValidationMethodOption],
                WaitTimeout = (int)accessor[blockIndex, WaitTimeoutOption],
                RemovalDelay = (int)accessor[blockIndex, RemovalDelayOption],
                ValidationTimeout = (int)accessor[blockIndex, ValidationTimeoutOption],
                SupportsOfflineValidation = (bool)accessor[blockIndex, SupportsOfflineValidationOption]
            };

            return true;
        }

        public void SetIdValidation(Identity identity)
        {
            if (Identity == identity)
            {
                return;
            }

            Identity = identity;

            var @event = new SetValidationEvent(IdReaderId, Identity);

            if (identity == null)
            {
                Fire(IdReaderLogicalStateTrigger.ValidationFailed, @event);
                Implementation.Eject();
            }
            else
            {
                if (!IsEgmControlled ||
                    _reader.IdReaderType == IdReaderTypes.None ||
                    _reader.Inserted && !string.IsNullOrEmpty(CardData))
                {
                    Fire(IdReaderLogicalStateTrigger.Validated, @event);
                    SetValidationComplete();
                }
            }

            TimeOfLastIdentityHandled = DateTime.UtcNow;
        }

        public void SetValidationComplete()
        {
            Implementation.ValidationComplete();

            if(_state.CanFire(IdReaderLogicalStateTrigger.Validated))
            {
                Fire(IdReaderLogicalStateTrigger.Validated);
            }
        }

        public void SetValidationFailed()
        {
            Implementation.ValidationFailed();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                Disable(DisabledReasons.Service);
                if (Implementation != null)
                {
                    Implementation.ResetFailed -= ResetFailed;
                    Implementation.ResetSucceeded -= ResetSucceeded;
                    Implementation.Connected -= ImplementationConnected;
                    Implementation.Disconnected -= ImplementationDisconnected;
                    Implementation.Initialized -= ImplementationInitialized;
                    Implementation.InitializationFailed -= ImplementationInitializationFailed;
                    Implementation.Disabled -= ImplementationDisabled;
                    Implementation.Enabled -= ImplementationEnabled;
                    Implementation.FaultCleared -= ImplementationFaultCleared;
                    Implementation.FaultOccurred -= ImplementationFaultOccurred;
                    Implementation.IdPresented -= ImplementationIdPresented;
                    Implementation.IdCleared -= ImplementationIdCleared;
                    Implementation.IdValidationRequested -= ImplementationIdValidationRequested;
                    Implementation.ReadError -= ImplementationReadError;
                    Implementation.Dispose();
                    _reader = null;
                }

                _timer.Dispose();

                _stateLock.Dispose();
            }

            _timer = null;
            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }


            base.Dispose(disposing);
        }

        /// <inheritdoc />
        protected override void DisabledDetected()
        {
            PostEvent(new DisabledEvent(IdReaderId, ReasonDisabled));
        }

        /// <inheritdoc />
        protected override void Disabling(DisabledReasons reason)
        {
            if (Fire(IdReaderLogicalStateTrigger.Disable, new DisabledEvent(IdReaderId, ReasonDisabled), true))
            {
                Implementation.Disable();
            }
        }

        /// <inheritdoc />
        protected override void Enabling(EnabledReasons reason, DisabledReasons remedied)
        {
            if (Enabled)
            {
                if (Fire(IdReaderLogicalStateTrigger.Enable, new EnabledEvent(IdReaderId, reason), true))
                {
                    Implementation.Enable();
                }
            }
            else
            {
                DisabledDetected();
            }
        }

        /// <inheritdoc />
        protected override void Initializing()
        {
            // Load an instance of the given protocol implementation.
            _reader = AddinFactory.CreateAddin<IIdReaderImplementation>(
                DeviceImplementationsExtensionPath,
                ServiceProtocol);
            if (Implementation == null)
            {
                var errorMessage = $"Cannot load {Name}";
                Logger.Fatal(errorMessage);
                throw new ServiceException(errorMessage);
            }

            ReadOrCreateOptions();

            Implementation.ResetFailed += ResetFailed;
            Implementation.ResetSucceeded += ResetSucceeded;
            Implementation.Connected += ImplementationConnected;
            Implementation.Disconnected += ImplementationDisconnected;
            Implementation.Initialized += ImplementationInitialized;
            Implementation.InitializationFailed += ImplementationInitializationFailed;
            Implementation.Disabled += ImplementationDisabled;
            Implementation.Enabled += ImplementationEnabled;
            Implementation.FaultCleared += ImplementationFaultCleared;
            Implementation.FaultOccurred += ImplementationFaultOccurred;
            Implementation.IdPresented += ImplementationIdPresented;
            Implementation.IdCleared += ImplementationIdCleared;
            Implementation.IdValidationRequested += ImplementationIdValidationRequested;
            Implementation.ReadError += ImplementationReadError;

            TimeOfLastIdentityHandled = DateTime.UtcNow;
        }

        /// <inheritdoc />
        protected override void Inspecting(IComConfiguration config, int timeout)
        {
            Fire(IdReaderLogicalStateTrigger.Inspecting);
        }

        /// <inheritdoc />
        protected override void SubscribeToEvents(IEventBus eventBus)
        {
        }

        private void ModifyBlock(string option, object value)
        {
            this.ModifyBlock(
                OptionsBlock,
                (transaction, index) =>
                {
                    transaction[index, option] = value;
                    return true;
                },
                IdReaderId - 1);
        }

        private bool IsExpired(DateTime lastAction)
        {
            return DateTime.UtcNow - lastAction > ValidationTimeoutTimeSpan;
        }

        private void CheckForTimeout(object sender, ElapsedEventArgs e)
        {
            if (_identity.ValidationExpired || !ShouldCheckForUserActivity)
            {
                return;
            }

            var lastAction = _userActivityService.GetLastAction();

            if (lastAction != null && IsExpired(lastAction.GetValueOrDefault()))
            {
                _identity.ValidationExpired = true;

                Logger.Debug(
                    $"Reader {IdReaderId} timeout. lastAction={lastAction} , ValidationTimeoutTimeSpan={ValidationTimeoutTimeSpan}");

                PostEvent(new IdReaderTimeoutEvent(IdReaderId));
            }
        }

        private void ReadOrCreateOptions()
        {
            if (!this.GetOrAddBlock(OptionsBlock, out var idReaderOptions, IdReaderId - 1))
            {
                return;
            }

            IdReaderTrack = idReaderOptions.IdReaderTrack;
            ValidationMethod = idReaderOptions.ValidationMethod;
            _waitTimeout = idReaderOptions.WaitTimeout;
            _removalDelay = idReaderOptions.RemovalDelay;
            _validationTimeout = idReaderOptions.ValidationTimeout;
            _supportsOfflineValidation = idReaderOptions.SupportsOfflineValidation;
        }

        private void ImplementationConnected(object sender, EventArgs e)
        {
            Logger.Info("ImplementationConnected: device connected");
            Enable(EnabledReasons.Device);
            Fire(IdReaderLogicalStateTrigger.Connected, new ConnectedEvent(IdReaderId));
        }

        private void ResetSucceeded(object sender, EventArgs e)
        {
            Logger.Info("ResetSucceeded: device successfully reset");
            Enable(EnabledReasons.Device);
        }

        private void ResetFailed(object sender, EventArgs e)
        {
            Logger.Info("ResetFailed: device reset failed");
            Disable(DisabledReasons.Device);
        }

        private void ImplementationDisconnected(object sender, EventArgs e)
        {
            Logger.Warn("ImplementationDisconnected: device disconnected");
            Disable(DisabledReasons.Device);
            Fire(IdReaderLogicalStateTrigger.Disconnected, new DisconnectedEvent(IdReaderId));
        }

        private void ImplementationInitializationFailed(object sender, EventArgs e)
        {
            Logger.Warn("ImplementationInitializationFailed: device initialization failed");
            Disable(DisabledReasons.Device);
            Fire(IdReaderLogicalStateTrigger.InspectionFailed, new InspectionFailedEvent(IdReaderId));
        }

        private void ImplementationInitialized(object sender, EventArgs e)
        {
            Logger.Info("ImplementationInitialized: device initialized");
            Fire(IdReaderLogicalStateTrigger.Initialized, new InspectedEvent(IdReaderId));
            Initialized = true;
            if (Enabled)
            {
                Implementation.Enable();
            }
            else
            {
                DisabledDetected();
                Implementation.Disable();
            }

            SetInternalConfiguration();
            Implementation.UpdateConfiguration(InternalConfiguration);
            RegisterComponent();
        }

        private void ImplementationDisabled(object sender, EventArgs e)
        {
            Logger.Warn("ImplementationDisabled: device disabled");
        }

        private void ImplementationEnabled(object sender, EventArgs e)
        {
            Logger.Info("ImplementationEnabled: device enabled");
        }

        private void ImplementationFaultCleared(object sender, FaultEventArgs e)
        {
            foreach (IdReaderFaultTypes value in Enum.GetValues(typeof(IdReaderFaultTypes)))
            {
                if ((e.Fault & value) != value)
                {
                    continue;
                }

                switch (value)
                {
                    case IdReaderFaultTypes.PowerFail:
                    case IdReaderFaultTypes.ComponentFault:
                    case IdReaderFaultTypes.FirmwareFault:
                    case IdReaderFaultTypes.OtherFault:
                        if (ClearError(value))
                        {
                            Logger.Info($"ImplementationFaultCleared: REMOVED {value} from the error list.");
                            if (!AnyErrors)
                            {
                                Enable(EnabledReasons.Reset);
                            }
                        }

                        break;
                    default:
                        continue;
                }

                PostEvent(new HardwareFaultClearEvent(IdReaderId, value));
            }
        }

        private void ImplementationFaultOccurred(object sender, FaultEventArgs e)
        {
            foreach (IdReaderFaultTypes value in Enum.GetValues(typeof(IdReaderFaultTypes)))
            {
                if ((e.Fault & value) != value)
                {
                    continue;
                }

                switch (value)
                {
                    case IdReaderFaultTypes.PowerFail:
                    case IdReaderFaultTypes.ComponentFault:
                    case IdReaderFaultTypes.FirmwareFault:
                    case IdReaderFaultTypes.OtherFault:
                        if (AddError(value))
                        {
                            Logger.Info($"ImplementationFaultOccurred: ADDED {value} to the error list.");
                            Disable(DisabledReasons.Error);
                        }
                        else
                        {
                            Logger.Debug($"ImplementationFaultOccurred: DUPLICATE ERROR EVENT {value}");
                        }

                        break;
                    default:
                        continue;
                }

                PostEvent(new HardwareFaultEvent(IdReaderId, value));
            }
        }

        private void ImplementationIdPresented(object sender, EventArgs e)
        {
            Fire(IdReaderLogicalStateTrigger.Presented, new IdPresentedEvent(IdReaderId));
        }

        private void ImplementationIdCleared(object sender, EventArgs e)
        {
            if (_reader.IdReaderType == IdReaderTypes.None ||
                !_reader.Inserted)
            {
                Fire(IdReaderLogicalStateTrigger.Cleared, new IdClearedEvent(IdReaderId));
            }

            TimeOfLastIdentityHandled = DateTime.UtcNow;
            if (ClearWarning(IdReaderLogicalState.BadRead))
            {
                Logger.Info($"REMOVED {IdReaderLogicalState.BadRead} from the warning list.");
            }
        }

        private void ImplementationIdValidationRequested(object sender, ValidationEventArgs e)
        {
            if (_reader.IdReaderType == IdReaderTypes.None ||
                _reader.Inserted)
            {
                Fire(IdReaderLogicalStateTrigger.Validating, new ValidationRequestedEvent(IdReaderId, e.TrackData));
            }

            TimeOfLastIdentityHandled = DateTime.UtcNow;
        }

        private void ImplementationReadError(object sender, EventArgs e)
        {
            Fire(IdReaderLogicalStateTrigger.ValidationFailed, new ReadErrorEvent(IdReaderId));
            if (AddWarning(IdReaderLogicalState.BadRead))
            {
                Logger.Info($"ADDED {IdReaderLogicalState.BadRead} to the warning list.");
            }

            Implementation.Eject();
        }

        private StateMachine<IdReaderLogicalState, IdReaderLogicalStateTrigger> ConfigureStateMachine()
        {
            var stateMachine =
                new StateMachine<IdReaderLogicalState, IdReaderLogicalStateTrigger>(IdReaderLogicalState.Uninitialized);
            // Uninitialized and Inspecting are only used when configuring the reader
            stateMachine.Configure(IdReaderLogicalState.Uninitialized)
                .Permit(IdReaderLogicalStateTrigger.Inspecting, IdReaderLogicalState.Inspecting)
                .PermitDynamic(
                    IdReaderLogicalStateTrigger.Initialized,
                    () => Enabled ? IdReaderLogicalState.Idle : IdReaderLogicalState.Disabled);

            stateMachine.Configure(IdReaderLogicalState.Inspecting)
                .Permit(IdReaderLogicalStateTrigger.InspectionFailed, IdReaderLogicalState.Uninitialized)
                .PermitDynamic(
                    IdReaderLogicalStateTrigger.Initialized,
                    () => Enabled ? IdReaderLogicalState.Idle : IdReaderLogicalState.Disabled)
                .Permit(IdReaderLogicalStateTrigger.Disconnected, IdReaderLogicalState.Disconnected);

            // when the reader is connected it goes through a reset before the event is fired so it is ready for Idle sate
            stateMachine.Configure(IdReaderLogicalState.Disconnected)
                .Permit(IdReaderLogicalStateTrigger.Connected, IdReaderLogicalState.Disabled)
                .Permit(IdReaderLogicalStateTrigger.Disable, IdReaderLogicalState.Disabled);

            stateMachine.Configure(IdReaderLogicalState.Idle)
                .Permit(IdReaderLogicalStateTrigger.Validating, IdReaderLogicalState.Validating)
                .Permit(IdReaderLogicalStateTrigger.Validated, IdReaderLogicalState.Validated)
                .PermitIf(
                    IdReaderLogicalStateTrigger.ValidationFailed,
                    IdReaderLogicalState.BadRead,
                    () => _reader.Inserted)
                .Permit(IdReaderLogicalStateTrigger.Presented, IdReaderLogicalState.Reading)
                .Permit(IdReaderLogicalStateTrigger.Disable, IdReaderLogicalState.Disabled)
                .Permit(IdReaderLogicalStateTrigger.Disconnected, IdReaderLogicalState.Disconnected);

            stateMachine.Configure(IdReaderLogicalState.Reading)
                .Permit(IdReaderLogicalStateTrigger.Cleared, IdReaderLogicalState.Idle)
                .Permit(IdReaderLogicalStateTrigger.Validating, IdReaderLogicalState.Validating)
                .Permit(IdReaderLogicalStateTrigger.Validated, IdReaderLogicalState.Validated)
                .Permit(IdReaderLogicalStateTrigger.ValidationFailed, IdReaderLogicalState.BadRead)
                .Permit(IdReaderLogicalStateTrigger.Disable, IdReaderLogicalState.Disabled)
                .Permit(IdReaderLogicalStateTrigger.Disconnected, IdReaderLogicalState.Disconnected);

            stateMachine.Configure(IdReaderLogicalState.Validating)
                .Permit(IdReaderLogicalStateTrigger.Cleared, IdReaderLogicalState.Idle)
                .Permit(IdReaderLogicalStateTrigger.Validated, IdReaderLogicalState.Validated)
                .Permit(IdReaderLogicalStateTrigger.ValidationFailed, IdReaderLogicalState.BadRead)
                .Permit(IdReaderLogicalStateTrigger.Disable, IdReaderLogicalState.Disabled)
                .Permit(IdReaderLogicalStateTrigger.Disconnected, IdReaderLogicalState.Disconnected);

            stateMachine.Configure(IdReaderLogicalState.Validated)
                .PermitReentry(IdReaderLogicalStateTrigger.Validated)
                .Permit(IdReaderLogicalStateTrigger.Validating, IdReaderLogicalState.Validating)
                .PermitIf(
                    IdReaderLogicalStateTrigger.ValidationFailed,
                    IdReaderLogicalState.BadRead,
                    () => _reader.Inserted)
                .Permit(IdReaderLogicalStateTrigger.Cleared, IdReaderLogicalState.Idle)
                .Permit(IdReaderLogicalStateTrigger.Disable, IdReaderLogicalState.Disabled)
                .Permit(IdReaderLogicalStateTrigger.Disconnected, IdReaderLogicalState.Disconnected);

            stateMachine.Configure(IdReaderLogicalState.BadRead)
                .PermitReentry(IdReaderLogicalStateTrigger.ValidationFailed)
                .Permit(IdReaderLogicalStateTrigger.Validated, IdReaderLogicalState.Validated)
                .Permit(IdReaderLogicalStateTrigger.Validating, IdReaderLogicalState.Validating)
                .Permit(IdReaderLogicalStateTrigger.Presented, IdReaderLogicalState.Reading)
                .Permit(IdReaderLogicalStateTrigger.Cleared, IdReaderLogicalState.Idle)
                .Permit(IdReaderLogicalStateTrigger.Disable, IdReaderLogicalState.Disabled)
                .Permit(IdReaderLogicalStateTrigger.Disconnected, IdReaderLogicalState.Disconnected);

            stateMachine.Configure(IdReaderLogicalState.Disabled)
                .PermitIf(IdReaderLogicalStateTrigger.Initialized, IdReaderLogicalState.Idle, () => !_reader.Inserted)
                .PermitIf(IdReaderLogicalStateTrigger.Initialized, IdReaderLogicalState.Validating, () => _reader.Inserted)
                .Permit(IdReaderLogicalStateTrigger.Enable, IdReaderLogicalState.Idle)
                .PermitReentry(IdReaderLogicalStateTrigger.Disable)
                .Permit(IdReaderLogicalStateTrigger.Disconnected, IdReaderLogicalState.Disconnected);

            stateMachine.Configure(IdReaderLogicalState.Error)
                .Permit(IdReaderLogicalStateTrigger.Cleared, IdReaderLogicalState.Idle)
                .Permit(IdReaderLogicalStateTrigger.Enable, IdReaderLogicalState.Idle)
                .Permit(IdReaderLogicalStateTrigger.Disable, IdReaderLogicalState.Disabled)
                .Permit(IdReaderLogicalStateTrigger.Disconnected, IdReaderLogicalState.Disconnected);

            stateMachine.OnUnhandledTrigger(
                (state, trigger) =>
                {
                    Logger.Error($"Invalid ID Reader State Transition. State : {state} Trigger : {trigger}");
                });

            stateMachine.OnTransitioned(
                transition =>
                {
                    Logger.Debug(
                        $"Transitioned From : {transition.Source} To : {transition.Destination} Trigger : {transition.Trigger}");
                });

            return stateMachine;
        }

        private bool Fire(IdReaderLogicalStateTrigger trigger, bool verify = false)
        {
            if (verify && !_state.CanFire(trigger))
            {
                Logger.Warn($"Cannot transition with trigger: {trigger}");
                return false;
            }

            Logger.Debug($"Transitioning with trigger: {trigger}");
            _stateLock.EnterWriteLock();
            try
            {
                _state.Fire(trigger);
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }

            return true;
        }

        private bool Fire<TEvent>(IdReaderLogicalStateTrigger trigger, TEvent @event, bool verify = false)
            where TEvent : IEvent
        {
            if (!Fire(trigger, verify))
            {
                return false;
            }

            if (@event != null)
            {
                PostEvent(@event);
            }

            return true;
        }
    }
}