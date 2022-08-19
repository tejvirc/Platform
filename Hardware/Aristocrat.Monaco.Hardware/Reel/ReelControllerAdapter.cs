namespace Aristocrat.Monaco.Hardware.Reel
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Contracts;
    using Contracts.Communicator;
    using Contracts.Gds.Reel;
    using Contracts.Persistence;
    using Contracts.Reel;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;
    using Stateless;

    public class ReelControllerAdapter : DeviceAdapter<IReelControllerImplementation>, IReelController,
        IStorageAccessor<ReelControllerOptions>
    {
        private const string DeviceImplementationsExtensionPath = "/Hardware/ReelController/ReelControllerImplementations";
        private const string OptionsBlock = "Aristocrat.Monaco.Hardware.MechanicalReels.ReelControllerAdapter.Options";
        private const string ReelBrightnessOption = "ReelBrightness";
        private const string ReelOffsetsOption = "ReelOffsets";
        private const int ReelOffsetDefaultValue = 0;
        private const int MaxBrightness = 100;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private static readonly IReadOnlyList<ReelLogicalState> NonIdleStates = new List<ReelLogicalState>
        {
            ReelLogicalState.Spinning,
            ReelLogicalState.Homing,
            ReelLogicalState.Stopping,
            ReelLogicalState.SpinningBackwards,
            ReelLogicalState.SpinningForward,
            ReelLogicalState.Tilted
        };

        private readonly StateMachine<ReelControllerState, ReelControllerTrigger> _state;
        private readonly ConcurrentDictionary<int, StateMachineWithStoppingTrigger> _reelStates = new();
        private readonly ReaderWriterLockSlim _stateLock = new(LockRecursionPolicy.SupportsRecursion);
        private readonly ConcurrentDictionary<int, int> _steps = new();

        private IReelControllerImplementation _reelController;

        private int _reelBrightness = MaxBrightness;
        private int[] _reelOffsets = new int[ReelConstants.MaxSupportedReels];

        public ReelControllerAdapter()
        {
            _state = CreateStateMachine();
        }

        public override DeviceType DeviceType => DeviceType.ReelController;

        public override string Name => !string.IsNullOrEmpty(ServiceProtocol)
            ? $"{ServiceProtocol} Reel Controller Service"
            : "Unknown Reel Controller Service";

        public override ICollection<Type> ServiceTypes => new List<Type> { typeof(IReelController) };

        public override bool Connected => Implementation?.IsConnected ?? false;

        /// <inheritdoc />
        public int ReelControllerId { get; set; } = 1; // Default to deviceId 1 since 0 isn't valid in G2S

        public ReelControllerFaults ReelControllerFaults =>
            Implementation?.ReelControllerFaults ?? new ReelControllerFaults();

        public IReadOnlyDictionary<int, ReelFaults> Faults =>
            Implementation?.Faults ?? new Dictionary<int, ReelFaults>();

        public IReadOnlyDictionary<int, ReelLogicalState> ReelStates
        {
            get
            {
                _stateLock.EnterReadLock();
                try
                {
                    return _reelStates.ToDictionary(x => x.Key, x => x.Value.StateMachine.State);
                }
                finally
                {
                    _stateLock.ExitReadLock();
                }
            }
        }

        public IReadOnlyDictionary<int, ReelStatus> ReelsStatus => Implementation?.ReelsStatus;

        public IReadOnlyDictionary<int, int> Steps => _steps;

        public ReelControllerState LogicalState
        {
            get
            {
                _stateLock.EnterReadLock();
                try
                {
                    return _state.State;
                }
                finally
                {
                    _stateLock.ExitReadLock();
                }
            }
        }

        public IReadOnlyCollection<int> ConnectedReels =>
            ReelStates.Where(x => x.Value != ReelLogicalState.Disconnected).Select(x => x.Key).ToList();

        public int DefaultReelBrightness
        {
            get => _reelBrightness;

            set
            {
                if (value is < 1 or > 100)
                {
                    return;
                }

                if (_reelBrightness == value)
                {
                    return;
                }

                _reelBrightness = value;
                this.ModifyBlock(
                    OptionsBlock,
                    (transaction, index) =>
                    {
                        transaction[index, ReelBrightnessOption] = _reelBrightness;
                        return true;
                    },
                    ReelControllerId - 1);
            }
        }

        public IEnumerable<int> ReelOffsets
        {
            get => _reelOffsets;

            set
            {
                var newValues = value.ToList();
                if (_reelOffsets.SequenceEqual(newValues))
                {
                    return;
                }

                while (newValues.Count < ReelConstants.MaxSupportedReels)
                {
                    newValues.Add(ReelOffsetDefaultValue);
                }
                _reelOffsets = newValues.ToArray();

                this.ModifyBlock(
                    OptionsBlock,
                    (transaction, index) =>
                    {
                        transaction[index, ReelOffsetsOption] = _reelOffsets;
                        return true;
                    },
                    ReelControllerId - 1);

                SetReelOffsets(_reelOffsets);
            }
        }

        public IReadOnlyDictionary<int, int> ReelHomeSteps { get; set; } = new Dictionary<int, int> { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 } };

        protected override IReelControllerImplementation Implementation => _reelController;

        protected override string Description => string.Empty;

        protected override string Path => string.Empty;

        private bool CanSendCommand => LogicalState is
                    not ReelControllerState.Uninitialized and
                    not ReelControllerState.IdleUnknown and
                    not ReelControllerState.Inspecting and
                    not ReelControllerState.Disabled and
                    not ReelControllerState.Homing;

        public Task<bool> SpinReels(params ReelSpinData[] reelData)
        {
            Logger.Debug("SpinReels with stops called");
            _stateLock.EnterWriteLock();
            bool canFire;
            bool fired = false;

            try
            {
                canFire = reelData.All(
                    reel => CanFire(
                        reel.Direction == SpinDirection.Forward
                            ? ReelControllerTrigger.SpinReel
                            : ReelControllerTrigger.SpinReelBackwards,
                        reel.ReelId));

                if (!canFire)
                {
                    Logger.Debug("SpinReels - CanFire FAILED - CAN NOT SPIN");
                }
                else
                {
                    fired = reelData.All(
                        reel => Fire(
                            reel.Direction == SpinDirection.Forward
                                ? ReelControllerTrigger.SpinReel
                                : ReelControllerTrigger.SpinReelBackwards,
                            reel.ReelId));

                    if (!fired)
                    {
                        Logger.Debug("SpinReels - Fire FAILED - CAN NOT SPIN");
                    }
                }
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }

            return canFire && fired
                ? Implementation?.SpinReels(reelData) ?? Task.FromResult(false)
                : Task.FromResult(false);
        }

        public Task<bool> SetLights(params ReelLampData[] lampData)
        {
            return CanSendCommand ? Implementation.SetLights(lampData) : Task.FromResult(false);
        }

        public Task<IList<int>> GetReelLightIdentifiers()
        {
            return Implementation.GetReelLightIdentifiers();
        }

        public Task<bool> SetReelBrightness(IReadOnlyDictionary<int, int> brightness)
        {
            return CanSendCommand ? Implementation.SetBrightness(brightness) : Task.FromResult(false);
        }

        public Task<bool> SetReelBrightness(int brightness)
        {
            return CanSendCommand ? Implementation.SetBrightness(brightness) : Task.FromResult(false);
        }

        public Task<bool> SetReelSpeed(params ReelSpeedData[] speedData)
        {
            return CanSendCommand ? Implementation.SetReelSpeed(speedData) : Task.FromResult(false);
        }

        public async Task<bool> HomeReels()
        {
            return await HomeReels(ReelHomeSteps);
        }

        public async Task<bool> HomeReels(IReadOnlyDictionary<int, int> reelOffsets)
        {
            Logger.Debug("HomeReels with stops called");

            if (!Fire(ReelControllerTrigger.HomeReels))
            {
                Logger.Debug("HomeReels - Fire FAILED - CAN NOT HOME");
                return false;
            }

            var allHomed = (await Task.WhenAll(
                    reelOffsets.Select(x =>
                    {
                        Logger.Debug($"homing reel {x.Key} to step {x.Value}");
                        if (!ConnectedReels.Contains(x.Key))
                        {
                            return Task.FromResult(true);
                        }

                        if (!Fire(ReelControllerTrigger.HomeReels, x.Key))
                        {
                            Logger.Debug($"HomeReels - Fire FAILED for reel {x.Key} - CAN NOT HOME");
                            return Task.FromResult(false);
                        }

                        return Implementation?.HomeReel(x.Key, x.Value) ?? Task.FromResult(false);
                    })))
                .All(x => x);

            if (!allHomed)
            {
                await TiltReels();
            }

            return allHomed;
        }

        public Task<bool> NudgeReel(params NudgeReelData[] reelData)
        {
            Logger.Debug("NudgeReels with stops called");
            _stateLock.EnterWriteLock();
            bool canFire;
            bool fired = false;

            try
            {
                canFire = reelData.All(
                    reel => CanFire(
                        reel.Direction == SpinDirection.Forward
                            ? ReelControllerTrigger.SpinReel
                            : ReelControllerTrigger.SpinReelBackwards,
                        reel.ReelId));

                if (!canFire)
                {
                    Logger.Debug("NudgeReel - CanFire FAILED - CAN NOT NUDGE");
                }
                else
                {
                    fired = reelData.All(
                        reel => Fire(
                            reel.Direction == SpinDirection.Forward
                                ? ReelControllerTrigger.SpinReel
                                : ReelControllerTrigger.SpinReelBackwards,
                            reel.ReelId));

                    if (!fired)
                    {
                        Logger.Debug("NudgeReel - Fire FAILED - CAN NOT NUDGE");
                    }
                }
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }

            return canFire && fired ? Implementation?.NudgeReels(reelData) ?? Task.FromResult(false) : Task.FromResult(false);
        }

        /// <inheritdoc />
        public bool TryAddBlock(IPersistentStorageAccessor accessor, int blockIndex, out ReelControllerOptions block)
        {
            block = new ReelControllerOptions { ReelBrightness = DefaultReelBrightness, ReelOffsets = ReelOffsets.ToArray() };

            using var transaction = accessor.StartTransaction();
            transaction[blockIndex, ReelBrightnessOption] = block.ReelBrightness;
            transaction[blockIndex, ReelOffsetsOption] = block.ReelOffsets;

            transaction.Commit();
            return true;
        }

        /// <inheritdoc />
        public bool TryGetBlock(IPersistentStorageAccessor accessor, int blockIndex, out ReelControllerOptions block)
        {
            block = new ReelControllerOptions
            {
                ReelBrightness = (int)accessor[blockIndex, ReelBrightnessOption],
                ReelOffsets = (int[])accessor[blockIndex, ReelOffsetsOption],
            };

            return true;
        }

        public Task<bool> TiltReels()
        {
            Logger.Debug("TiltReels called");
            FireAll(ReelControllerTrigger.TiltReels);
            return Implementation?.TiltReels() ?? Task.FromResult(false);
        }

        protected override void DisabledDetected()
        {
            PostEvent(new DisabledEvent(ReelControllerId, ReasonDisabled));
        }

        protected override void Disabling(DisabledReasons reason)
        {
            if (LogicalState == ReelControllerState.Uninitialized)
            {
                Logger.Warn($"Disable for {ReasonDisabled} ignored for state {LogicalState}");
                return;
            }

            if (!Fire(ReelControllerTrigger.Disable, new DisabledEvent(ReelControllerId, ReasonDisabled)))
            {
                return;
            }

            Logger.Debug($"Disabling for {ReasonDisabled}");
            Implementation?.Disable();
        }

        protected override void Enabling(EnabledReasons reason, DisabledReasons remedied)
        {
            if (Enabled)
            {
                if (Fire(ReelControllerTrigger.Enable, new EnabledEvent(ReelControllerId, reason)))
                {
                    Implementation?.Enable();
                }
                else
                {
                    PostEvent(new EnabledEvent(ReelControllerId, reason));
                }
            }
            else
            {
                if (reason == EnabledReasons.Device &&
                    ((remedied & DisabledReasons.GamePlay) != 0 ||
                     (ReasonDisabled & DisabledReasons.GamePlay) != 0 ||
                     (ReasonDisabled & DisabledReasons.System) != 0))
                {
                    PostEvent(new EnabledEvent(reason));
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                if (Implementation != null)
                {
                    Implementation.FaultCleared -= ReelControllerFaultCleared;
                    Implementation.ReelDisconnected -= ReelControllerOnReelDisconnected;
                    Implementation.ReelConnected -= ReelControllerOnReelConnected;
                    Implementation.FaultOccurred -= ReelControllerFaultOccurred;
                    Implementation.ControllerFaultOccurred -= ReelControllerFaultOccurred;
                    Implementation.ControllerFaultCleared -= ReelControllerFaultCleared;
                    Implementation.ReelSlowSpinning -= ReelControllerSlowSpinning;
                    Implementation.ReelStopped -= ReelControllerReelStopped;
                    Implementation.ReelSpinning -= ReelControllerSpinning;
                    Implementation.Connected -= ReelControllerConnected;
                    Implementation.Disconnected -= ReelControllerDisconnected;
                    Implementation.Disabled -= ReelControllerDisabled;
                    Implementation.Enabled -= ReelControllerEnabled;
                    Implementation.InitializationFailed -= ReelControllerInitializationFailed;
                    Implementation.Initialized -= ReelControllerInitialized;
                    Implementation.HardwareInitialized -= HardwareInitialized;
                    Implementation.Dispose();
                    _reelController = null;
                }

                _stateLock.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void Initializing()
        {
            _reelController = AddinFactory.CreateAddin<IReelControllerImplementation>(
                DeviceImplementationsExtensionPath,
                ServiceProtocol);
            if (_reelController == null)
            {
                throw new InvalidOperationException("reel controller addin not available");
            }

            ReadOrCreateOptions();

            Implementation.FaultCleared += ReelControllerFaultCleared;
            Implementation.FaultOccurred += ReelControllerFaultOccurred;
            Implementation.ControllerFaultOccurred += ReelControllerFaultOccurred;
            Implementation.ControllerFaultCleared += ReelControllerFaultCleared;
            Implementation.ReelSlowSpinning += ReelControllerSlowSpinning;
            Implementation.ReelStopped += ReelControllerReelStopped;
            Implementation.ReelSpinning += ReelControllerSpinning;
            Implementation.Connected += ReelControllerConnected;
            Implementation.Disconnected += ReelControllerDisconnected;
            Implementation.Disabled += ReelControllerDisabled;
            Implementation.Enabled += ReelControllerEnabled;
            Implementation.InitializationFailed += ReelControllerInitializationFailed;
            Implementation.Initialized += ReelControllerInitialized;
            Implementation.ReelDisconnected += ReelControllerOnReelDisconnected;
            Implementation.ReelConnected += ReelControllerOnReelConnected;
            Implementation.HardwareInitialized += HardwareInitialized;
        }

        protected override void Inspecting(IComConfiguration comConfiguration, int timeout)
        {
            Fire(ReelControllerTrigger.Inspecting);
        }

        protected override void SubscribeToEvents(IEventBus eventBus)
        {
        }

        private StateMachineWithStoppingTrigger CreateReelStateMachine(ReelLogicalState state = ReelLogicalState.IdleUnknown)
        {
            var stateMachine = new StateMachine<ReelLogicalState, ReelControllerTrigger>(state);
            var reelStoppedTriggerParameters = stateMachine.SetTriggerParameters<ReelLogicalState, ReelEventArgs>(ReelControllerTrigger.ReelStopped);

            stateMachine.Configure(ReelLogicalState.IdleUnknown)
                .Permit(ReelControllerTrigger.Disconnected, ReelLogicalState.Disconnected)
                .Permit(ReelControllerTrigger.TiltReels, ReelLogicalState.Tilted)
                .Permit(ReelControllerTrigger.HomeReels, ReelLogicalState.Homing)
                .Ignore(ReelControllerTrigger.Connected);

            stateMachine.Configure(ReelLogicalState.IdleAtStop)
                .Permit(ReelControllerTrigger.Disconnected, ReelLogicalState.Disconnected)
                .Permit(ReelControllerTrigger.TiltReels, ReelLogicalState.Tilted)
                .Permit(ReelControllerTrigger.SpinReel, ReelLogicalState.SpinningForward)
                .Permit(ReelControllerTrigger.SpinReelBackwards, ReelLogicalState.SpinningBackwards)
                .Permit(ReelControllerTrigger.HomeReels, ReelLogicalState.Homing)
                .OnEntryFrom(reelStoppedTriggerParameters, (logicalState, args) => PostEvent(new ReelStoppedEvent(args.ReelId, args.Step, logicalState == ReelLogicalState.Homing)));

            stateMachine.Configure(ReelLogicalState.Tilted)
                .Ignore(ReelControllerTrigger.TiltReels)
                .Permit(ReelControllerTrigger.HomeReels, ReelLogicalState.Homing)
                .Permit(ReelControllerTrigger.Disconnected, ReelLogicalState.Disconnected);

            stateMachine.Configure(ReelLogicalState.Spinning)
                .Permit(ReelControllerTrigger.Disconnected, ReelLogicalState.Disconnected)
                .Permit(ReelControllerTrigger.ReelStopped, ReelLogicalState.IdleAtStop)
                .Permit(ReelControllerTrigger.TiltReels, ReelLogicalState.Tilted);

            stateMachine.Configure(ReelLogicalState.Homing)
                .SubstateOf(ReelLogicalState.Spinning);

            stateMachine.Configure(ReelLogicalState.Disconnected)
                .Permit(ReelControllerTrigger.Connected, ReelLogicalState.IdleUnknown);

            stateMachine.Configure(ReelLogicalState.SpinningForward).SubstateOf(ReelLogicalState.Spinning);
            stateMachine.Configure(ReelLogicalState.SpinningBackwards).SubstateOf(ReelLogicalState.Spinning);

            stateMachine.OnTransitioned(
                transition =>
                {
                    Logger.Debug(
                        $"ReelStateMachine(Reel) - Trigger {transition.Trigger} Transitioned From {transition.Source} To {transition.Destination}");
                });

            stateMachine.OnUnhandledTrigger(
                (unHandledState, trigger) =>
                {
                    Logger.Error($"ReelStateMachine(Reel) - Invalid State {unHandledState} For Trigger {trigger}");
                });

            return new StateMachineWithStoppingTrigger(stateMachine, reelStoppedTriggerParameters);
        }

        private StateMachine<ReelControllerState, ReelControllerTrigger> CreateStateMachine()
        {
            var stateMachine = new StateMachine<ReelControllerState, ReelControllerTrigger>(ReelControllerState.Uninitialized);
            stateMachine.Configure(ReelControllerState.Uninitialized)
                .Permit(ReelControllerTrigger.Inspecting, ReelControllerState.Inspecting)
                .Permit(ReelControllerTrigger.Disable, ReelControllerState.Disabled)
                .PermitDynamic(
                    ReelControllerTrigger.Initialized,
                    () => Enabled ? ReelControllerState.IdleUnknown : ReelControllerState.Disabled);

            stateMachine.Configure(ReelControllerState.Inspecting)
                .Ignore(ReelControllerTrigger.Disable)
                .Ignore(ReelControllerTrigger.ReelStopped)
                .Permit(ReelControllerTrigger.InspectionFailed, ReelControllerState.Uninitialized)
                .PermitDynamic(
                    ReelControllerTrigger.Initialized,
                    () => Enabled ? ReelControllerState.IdleUnknown : ReelControllerState.Disabled)
                .Permit(ReelControllerTrigger.Disconnected, ReelControllerState.Disconnected);

            stateMachine.Configure(ReelControllerState.IdleUnknown)
                .Ignore(ReelControllerTrigger.Connected)
                .Ignore(ReelControllerTrigger.Enable)
                .Ignore(ReelControllerTrigger.Initialized)
                .Permit(ReelControllerTrigger.Disconnected, ReelControllerState.Disconnected)
                .Permit(ReelControllerTrigger.TiltReels, ReelControllerState.Tilted)
                .Permit(ReelControllerTrigger.HomeReels, ReelControllerState.Homing)
                .Permit(ReelControllerTrigger.Disable, ReelControllerState.Disabled);

            stateMachine.Configure(ReelControllerState.IdleAtStops)
                .Ignore(ReelControllerTrigger.Connected)
                .Ignore(ReelControllerTrigger.Enable)
                .Ignore(ReelControllerTrigger.Initialized)
                .Permit(ReelControllerTrigger.Disconnected, ReelControllerState.Disconnected)
                .Permit(ReelControllerTrigger.SpinReel, ReelControllerState.Spinning)
                .Permit(ReelControllerTrigger.SpinReelBackwards, ReelControllerState.Spinning)
                .Permit(ReelControllerTrigger.TiltReels, ReelControllerState.Tilted)
                .Permit(ReelControllerTrigger.HomeReels, ReelControllerState.Homing)
                .Permit(ReelControllerTrigger.Disable, ReelControllerState.Disabled);

            stateMachine.Configure(ReelControllerState.Homing)
                .PermitReentry(ReelControllerTrigger.HomeReels)
                .Permit(ReelControllerTrigger.Disable, ReelControllerState.Disabled)
                .Permit(ReelControllerTrigger.Disconnected, ReelControllerState.Disconnected)
                .PermitDynamic(
                    ReelControllerTrigger.ReelStopped,
                    () => ReelStates.All(x => x.Value == ReelLogicalState.IdleAtStop)
                        ? ReelControllerState.IdleAtStops
                        : ReelControllerState.Homing)
                .Permit(ReelControllerTrigger.TiltReels, ReelControllerState.Tilted);

            stateMachine.Configure(ReelControllerState.Tilted)
                .SubstateOf(ReelControllerState.Disabled)
                .Permit(ReelControllerTrigger.HomeReels, ReelControllerState.Homing);

            stateMachine.Configure(ReelControllerState.Disabled)
                .Ignore(ReelControllerTrigger.ReelStopped)
                .Permit(ReelControllerTrigger.TiltReels, ReelControllerState.Tilted)
                .Permit(ReelControllerTrigger.Disconnected, ReelControllerState.Disconnected)
                .Ignore(ReelControllerTrigger.Disable)
                .PermitDynamic(
                    ReelControllerTrigger.Enable,
                    GetEnabledState);

            stateMachine.Configure(ReelControllerState.Disconnected)
                .Permit(ReelControllerTrigger.Connected, ReelControllerState.Inspecting);

            stateMachine.Configure(ReelControllerState.Spinning)
                .PermitReentry(ReelControllerTrigger.SpinReel)
                .PermitReentry(ReelControllerTrigger.SpinReelBackwards)
                .Permit(ReelControllerTrigger.TiltReels, ReelControllerState.Tilted)
                .Permit(ReelControllerTrigger.Disconnected, ReelControllerState.Disconnected)
                .PermitDynamic(
                    ReelControllerTrigger.ReelStopped,
                    () => ReelStates.All(x => x.Value == ReelLogicalState.IdleAtStop)
                        ? ReelControllerState.IdleAtStops
                        : ReelControllerState.Spinning);

            stateMachine.OnUnhandledTrigger(
                (state, trigger) =>
                {
                    Logger.Error($"Invalid State {state} For Trigger {trigger}");
                });

            stateMachine.OnTransitioned(
                transition =>
                {
                    Logger.Debug(
                        $"ReelControllerStateMachine - Trigger {transition.Trigger} Transitioned From {transition.Source} To {transition.Destination}");
                });

            return stateMachine;
        }

        private ReelControllerState GetEnabledState()
        {
            var nonIdleReels = ReelStates.Where(x => NonIdleStates.Contains(x.Value)).ToList();
            if (nonIdleReels.Any())
            {
                return nonIdleReels.FirstOrDefault().Value switch
                {
                    ReelLogicalState.Spinning or ReelLogicalState.SpinningBackwards or ReelLogicalState.SpinningForward
                        or ReelLogicalState.Stopping => ReelControllerState.Spinning,
                    ReelLogicalState.Homing => ReelControllerState.Homing,
                    ReelLogicalState.Tilted => ReelControllerState.Tilted,
                    _ => ReelStates.All(x => x.Value == ReelLogicalState.IdleAtStop)
                        ? ReelControllerState.IdleAtStops
                        : ReelControllerState.IdleUnknown
                };
            }

            return ReelStates.All(x => x.Value == ReelLogicalState.IdleAtStop)
                ? ReelControllerState.IdleAtStops
                : ReelControllerState.IdleUnknown;
        }

        private bool CanFire(ReelControllerTrigger trigger)
        {
            _stateLock.EnterReadLock();
            try
            {
                var canFire = _state.CanFire(trigger);
                if (!canFire)
                {
                    Logger.Debug($"CanFire - FAILED for trigger {trigger} in state {_state.State}");
                }

                return canFire;
            }
            finally
            {
                _stateLock.ExitReadLock();
            }
        }

        private bool FireAll(ReelControllerTrigger trigger)
        {
            _stateLock.EnterWriteLock();
            try
            {
                var reels = ConnectedReels;
                var fireAll = reels.All(reel => CanFire(trigger, reel)) &&
                              reels.All(reel => Fire(trigger, reel)) &&
                              (reels.Any() || Fire(trigger)); // If we have reels don't transition the state it is handled above

                if (!fireAll)
                {
                    Logger.Debug($"FireAll - FAILED for trigger {trigger}");
                }

                return fireAll;
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        private bool Fire(ReelControllerTrigger trigger)
        {
            _stateLock.EnterWriteLock();
            try
            {
                if (!CanFire(trigger))
                {
                    Logger.Debug($"Fire - FAILED CanFire for trigger {trigger}");
                    return false;
                }

                _state.Fire(trigger);
                return true;
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        private bool Fire<TEvent>(ReelControllerTrigger trigger, TEvent @event)
            where TEvent : BaseEvent
        {
            _stateLock.EnterWriteLock();
            try
            {
                if (!Fire(trigger))
                {
                    Logger.Debug($"Fire - FAILED for trigger {trigger}");
                    return false;
                }

                if (@event != null)
                {
                    PostEvent(@event);
                }

                return true;
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        private bool CanFire(ReelControllerTrigger trigger, int reelId, bool checkControllerState = true)
        {
            _stateLock.EnterReadLock();
            bool canFire;
            try
            {
                canFire = !checkControllerState || CanFire(trigger);
                if (!canFire)
                {
                    Logger.Debug($"CanFire - FAILED for trigger {trigger} and reel {reelId} in state {_state.State}");
                }
                else
                {
                    canFire = _reelStates.TryGetValue(reelId, out var reelState);
                    if (!canFire)
                    {
                        Logger.Debug($"CanFire - FAILED getting reel state for trigger {trigger} and reel {reelId} in state {_state.State}");
                    }
                    else
                    {
                        canFire = reelState.StateMachine.CanFire(trigger);
                        if (!canFire)
                        {
                            Logger.Debug($"CanFire - FAILED for trigger {trigger} and reel {reelId} with reelState {reelState.StateMachine} in state {_state.State}");
                        }
                    }
                }
            }
            finally
            {
                _stateLock.ExitReadLock();
            }

            return canFire;
        }

        private bool Fire(ReelControllerTrigger trigger, int reelId, bool updateControllerState = true)
        {
            _stateLock.EnterWriteLock();
            try
            {
                var canFire = _reelStates.TryGetValue(reelId, out var reelState);
                if (!canFire)
                {
                    Logger.Debug($"Fire - FAILED getting reel state for trigger {trigger} and reel {reelId} in state {_state.State}");
                    return false;
                }
                else
                {
                    canFire = CanFire(trigger, reelId, updateControllerState);
                    if (!canFire)
                    {
                        Logger.Debug($"Fire - FAILED CanFire for trigger {trigger} and reel {reelId} with reelState {reelState.StateMachine} in state {_state.State}");
                        return false;
                    }
                    else if (updateControllerState)
                    {
                        canFire = CanFire(trigger);
                        if (!canFire)
                        {
                            Logger.Debug($"Fire - FAILED CanFire for trigger {trigger} with reelState {reelState.StateMachine} in state {_state.State}");
                            return false;
                        }
                    }
                }

                reelState.StateMachine.Fire(trigger);
                return !updateControllerState || Fire(trigger);
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        private bool FireReelStopped(ReelControllerTrigger trigger, int reelId, ReelEventArgs args)
        {
            Logger.Debug($"FireReelStopped with trigger {trigger} for reel {reelId}");
            _stateLock.EnterWriteLock();
            try
            {
                var canFire = _reelStates.TryGetValue(reelId, out var reelState);
                if (!canFire)
                {
                    Logger.Debug($"FireReelStopped - FAILED getting reel state for trigger {trigger} and reel {reelId} in state {_state.State}");
                    return false;
                }
                else
                {
                    canFire = CanFire(trigger, reelId);
                    if (!canFire)
                    {
                        Logger.Debug($"FireReelStopped - FAILED CanFire for trigger {trigger} and reel {reelId} with reelState {reelState.StateMachine} in state {_state.State}");
                        return false;
                    }
                }

                Logger.Debug($"Stopping with trigger {reelState.StoppingTrigger} from state {reelState.StateMachine.State}");
                reelState.StateMachine.Fire(reelState.StoppingTrigger, reelState.StateMachine.State, args);
                return Fire(trigger);
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        private void Fire<TEvent>(
            ReelControllerTrigger trigger,
            int reelId,
            TEvent @event,
            bool updateControllerState = true)
            where TEvent : IEvent
        {
            _stateLock.EnterWriteLock();
            try
            {
                if (!Fire(trigger, reelId, updateControllerState))
                {
                    Logger.Debug($"Fire - FAILED for trigger {trigger} and reel {reelId} with updateControllerState {updateControllerState}");
                    return;
                }

                PostEvent(@event);
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        private void ReelControllerOnReelConnected(object sender, ReelEventArgs e)
        {
            _stateLock.EnterWriteLock();
            try
            {
                _reelStates.AddOrUpdate(
                    e.ReelId,
                    i =>
                    {
                        PostEvent(new ReelConnectedEvent(i, ReelControllerId));
                        SetInitialReelBrightness(e.ReelId);
                        return CreateReelStateMachine();
                    },
                    (i, s) =>
                    {
                        Fire(ReelControllerTrigger.Connected, i, new ReelConnectedEvent(i, ReelControllerId), false);
                        SetInitialReelBrightness(e.ReelId);
                        return s;
                    });
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        private void ReelControllerOnReelDisconnected(object sender, ReelEventArgs e)
        {
            if (!Connected)
            {
                return;
            }

            _stateLock.EnterWriteLock();
            try
            {
                _reelStates.AddOrUpdate(
                    e.ReelId,
                    i =>
                    {
                        PostEvent(new ReelDisconnectedEvent(i, ReelControllerId));
                        return CreateReelStateMachine(ReelLogicalState.Disconnected);
                    },
                    (i, s) =>
                    {
                        Fire(ReelControllerTrigger.Disconnected, i, new ReelDisconnectedEvent(i, ReelControllerId), false);
                        return s;
                    });
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        private void ReelControllerSpinning(object sender, ReelEventArgs e)
        {
            Logger.Debug($"ReelControllerSpinning reel {e.ReelId}");
            Fire(ReelControllerTrigger.SpinReel, e.ReelId);
        }

        private void ReelControllerReelStopped(object sender, ReelEventArgs e)
        {
            _steps[e.ReelId] = e.Step;
            if (!FireReelStopped(ReelControllerTrigger.ReelStopped, e.ReelId, e))
            {
                Logger.Debug($"ReelControllerReelStopped - FAILED FireReelStopped for reel {e.ReelId}");
            }
        }

        private void ReelControllerInitialized(object sender, EventArgs e)
        {
            if (!CanFire(ReelControllerTrigger.Initialized))
            {
                return;
            }

            if ((ReasonDisabled & DisabledReasons.Device) != 0)
            {
                Enable(EnabledReasons.Device);
            }

            // If we are also disabled for error, clear it so that we enable for reset below.
            if ((ReasonDisabled & DisabledReasons.Error) != 0)
            {
                ClearError(DisabledReasons.Error);
            }

            SetInternalConfiguration();
            Implementation?.UpdateConfiguration(InternalConfiguration);
            RegisterComponent();
            Initialized = true;

            InitializeReels().WaitForCompletion();
            SetReelOffsets(_reelOffsets.ToArray());

            Fire(ReelControllerTrigger.Initialized, new InspectedEvent(ReelControllerId));
            if (Enabled)
            {
                Implementation?.Enable();
            }
            else
            {
                DisabledDetected();
                Implementation?.Disable();
            }
        }

        private void HardwareInitialized(object sender, EventArgs e)
        {
            PostEvent(new HardwareInitializedEvent());
        }

        private async Task InitializeReels()
        {
            if (await CalculateCrc(0) == 0 || !await SelfTest(false))
            {
                return;
            }

            if (Enable(EnabledReasons.Device))
            {
                return;
            }

            if (!AnyErrors && (ReasonDisabled & DisabledReasons.Error) != 0)
            {
                Enable(EnabledReasons.Reset);
            }
        }

        private void ReelControllerInitializationFailed(object sender, EventArgs e)
        {
            if (Implementation != null)
            {
                SetInternalConfiguration();

                Implementation.UpdateConfiguration(InternalConfiguration);
            }

            Logger.Warn("ReelControllerInitializationFailed - Inspection Failed");
            Disable(DisabledReasons.Device);

            Fire(ReelControllerTrigger.InspectionFailed, new InspectionFailedEvent(ReelControllerId));
            PostEvent(new DisabledEvent(DisabledReasons.Error));
        }

        private void ReelControllerEnabled(object sender, EventArgs e)
        {
            Fire(ReelControllerTrigger.Enable);
        }

        private void ReelControllerDisabled(object sender, EventArgs e)
        {
            Logger.Debug("ReelControllerDisabled called");
            Fire(ReelControllerTrigger.Disable);
        }

        private void ReelControllerDisconnected(object sender, EventArgs e)
        {
            _stateLock.EnterWriteLock();
            try
            {
                _reelStates.Clear();
                Disable(DisabledReasons.Device);
                Fire(ReelControllerTrigger.Disconnected, new DisconnectedEvent(ReelControllerId));
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        private void ReelControllerConnected(object sender, EventArgs e)
        {
            Fire(ReelControllerTrigger.Connected, new ConnectedEvent(ReelControllerId));
        }

        private void ReelControllerSlowSpinning(object sender, ReelEventArgs e)
        {
            Logger.Debug($"ReelControllerSlowSpinning {e.ReelId}");

            _stateLock.EnterWriteLock();
            try
            {
                if (!_reelStates.TryGetValue(e.ReelId, out _))
                {
                    Logger.Warn($"ReelControllerSlowSpinning Ignoring event for invalid reel: {e.ReelId}");
                    return;
                }

                Fire(ReelControllerTrigger.TiltReels, e.ReelId);
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        private void ReelControllerFaultOccurred(object sender, ReelControllerFaultedEventArgs e)
        {
            if (e.Faults == ReelControllerFaults.None)
            {
                return;
            }

            AddError(e.Faults);

            Logger.Info($"ReelControllerFaultOccurred - ADDED {e.Faults} from the error list");
            Disable(DisabledReasons.Error);

            PostEvent(new HardwareFaultEvent(e.Faults));
        }

        private void ReelControllerFaultCleared(object sender, ReelControllerFaultedEventArgs e)
        {
            if (e.Faults == ReelControllerFaults.None)
            {
                return;
            }

            ClearError(e.Faults);

            Logger.Info($"ReelControllerFaultCleared - REMOVED {e.Faults} from the error list");
            if (!AnyErrors)
            {
                Enable(EnabledReasons.Reset);
            }

            PostEvent(new HardwareFaultClearEvent(ReelControllerId, e.Faults));
        }

        private void ReelControllerFaultOccurred(object sender, ReelFaultedEventArgs e)
        {
            // Do not process reel fault events for invalid reels
            if (!ConnectedReels.Contains(e.ReelId))
            {
                Logger.Info($"ReelControllerFaultOccurred - Ignoring event for invalid reel {e.ReelId}");
                return;
            }

            foreach (var value in e.Faults.GetFlags().Where(x => x != ReelFaults.None))
            {
                if (!AddError(value))
                {
                    Logger.Info($"ReelControllerFaultOccurred - SKIPPED {value} already added to list for reel {e.ReelId}");
                    continue;
                }

                Logger.Info($"ReelControllerFaultOccurred - ADDED {value} to the error list for reel {e.ReelId}");
                Disable(DisabledReasons.Error);

                PostEvent(
                    e.Faults is ReelFaults.ReelStall or ReelFaults.ReelTamper
                        ? new HardwareReelFaultEvent(ReelControllerId, value, e.ReelId)
                        : new HardwareReelFaultEvent(ReelControllerId, value));
            }
        }

        private void ReelControllerFaultCleared(object sender, ReelFaultedEventArgs e)
        {
            // Do not process reel fault cleared events for invalid reels
            if (!ConnectedReels.Contains(e.ReelId))
            {
                Logger.Info($"ReelControllerFaultCleared - Ignoring event for invalid reel {e.ReelId}");
                return;
            }

            var clearedFaults = new List<ReelFaults>();
            foreach (var value in e.Faults.GetFlags().Where(x => x != ReelFaults.None))
            {
                if (!ClearError(value))
                {
                    Logger.Info($"ReelControllerFaultCleared - SKIPPED {value} not in list for reel {e.ReelId}");
                    continue;
                }

                Logger.Info($"ReelControllerFaultCleared - REMOVED {value} from the error list for reel {e.ReelId}");
                if (!AnyErrors)
                {
                    Enable(EnabledReasons.Reset);
                }

                clearedFaults.Add(value);
            }

            foreach (var clearedFault in clearedFaults)
            {
                PostEvent(new HardwareReelFaultClearEvent(ReelControllerId, clearedFault));
            }
        }

        private void SetInitialReelBrightness(int reelId)
        {
            SetReelBrightness(new Dictionary<int, int> { { reelId, DefaultReelBrightness } });
        }

        private void ReadOrCreateOptions()
        {
            if (!this.GetOrAddBlock(OptionsBlock, out var options, ReelControllerId - 1))
            {
                Logger.Error($"Could not access block {OptionsBlock} {ReelControllerId - 1}");
                return;
            }

            _reelBrightness = options.ReelBrightness;
            _reelOffsets = options.ReelOffsets;

            Logger.Debug($"Block successfully read {OptionsBlock} {ReelControllerId - 1}");
        }

        private void SetReelOffsets(params int[] offsets)
        {
            Implementation.SetReelOffsets(offsets);
        }

        private class StateMachineWithStoppingTrigger
        {
            public StateMachine<ReelLogicalState, ReelControllerTrigger> StateMachine { get; }

            public StateMachine<ReelLogicalState, ReelControllerTrigger>.TriggerWithParameters<ReelLogicalState, ReelEventArgs> StoppingTrigger { get; }

            public StateMachineWithStoppingTrigger(
                StateMachine<ReelLogicalState, ReelControllerTrigger> stateMachine,
                StateMachine<ReelLogicalState, ReelControllerTrigger>.TriggerWithParameters<ReelLogicalState, ReelEventArgs> stoppingTrigger)
            {
                StateMachine = stateMachine;
                StoppingTrigger = stoppingTrigger;
            }
        }
    }
}