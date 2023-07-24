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
    using Contracts.Persistence;
    using Contracts.Reel;
    using Contracts.Reel.Capabilities;
    using Contracts.Reel.Events;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;

    public class ReelControllerAdapter : DeviceAdapter<IReelControllerImplementation>,
        IReelController,
        IStorageAccessor<ReelControllerOptions>
    {
        private const string DeviceImplementationsExtensionPath = "/Hardware/ReelController/ReelControllerImplementations";
        private const string OptionsBlock = "Aristocrat.Monaco.Hardware.MechanicalReels.ReelControllerAdapter.Options";
        private const string ReelOffsetsOption = "ReelOffsets";
        private const string ReelController = "Reel Controller";
        private const int ReelOffsetDefaultValue = 0;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly SemaphoreSlim _reelSpinningLock = new(1, 1);
        private readonly ConcurrentDictionary<int, int> _steps = new();

        private IReelControllerImplementation _reelControllerImplementation;
        private Dictionary<Type, IReelControllerCapability> _supportedCapabilities = new();
        private ReelControllerStateManager _stateManager;
        private int[] _reelOffsets = new int[ReelConstants.MaxSupportedReels];
        private int _controllerId = 1;

        public ReelControllerAdapter()
        {
            _stateManager = new ReelControllerStateManager(ReelControllerId, () => Enabled);
        }

        public override DeviceType DeviceType => DeviceType.ReelController;

        public override string Name => !string.IsNullOrEmpty(ServiceProtocol)
            ? $"{ServiceProtocol} Reel Controller Service"
            : "Unknown Reel Controller Service";

        public override ICollection<Type> ServiceTypes => new List<Type> { typeof(IReelController) };

        public override bool Connected => Implementation?.IsConnected ?? false;

        public int ReelControllerId
        {
            get => _controllerId;

            set
            {
                _controllerId = value;

                if (_stateManager is not null)
                {
                    _stateManager.ControllerId = value;
                }
            }
        }

        public ReelControllerFaults ReelControllerFaults =>
            Implementation?.ReelControllerFaults ?? new ReelControllerFaults();

        public IReadOnlyDictionary<int, ReelFaults> Faults =>
            Implementation?.Faults ?? new Dictionary<int, ReelFaults>();

        public IReadOnlyDictionary<int, ReelLogicalState> ReelStates => _stateManager?.ReelStates;

        public IReadOnlyDictionary<int, ReelStatus> ReelsStatus => Implementation?.ReelStatuses;

        public IReadOnlyDictionary<int, int> Steps => _steps;

        public ReelControllerState LogicalState => _stateManager?.LogicalState ?? ReelControllerState.Disconnected;

        public IReadOnlyCollection<int> ConnectedReels => _stateManager?.ConnectedReels;

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

        protected override IReelControllerImplementation Implementation => _reelControllerImplementation;

        protected override string Description => ReelController;

        protected override string Path => Kernel.Contracts.Components.Constants.ReelControllerPath;

        public bool HasCapability<T>() where T : class, IReelControllerCapability => _supportedCapabilities.ContainsKey(typeof(T));

        public T GetCapability<T>() where T : class, IReelControllerCapability
        {
            if (!HasCapability<T>())
            {
                throw new InvalidOperationException("capability not available");
            }

            return _supportedCapabilities[typeof(T)] as T;
        }

        public IEnumerable<Type> GetCapabilities() => _supportedCapabilities.Keys;

        public async Task<bool> HomeReels()
        {
            return await HomeReels(ReelHomeSteps);
        }

        public Task<bool> HomeReels(IReadOnlyDictionary<int, int> reelOffsets)
        {
            return HandleReelSpinningActionAsync(() => HomeReelsInternal(reelOffsets));
        }

        public bool TryAddBlock(IPersistentStorageAccessor accessor, int blockIndex, out ReelControllerOptions block)
        {
            block = new ReelControllerOptions { ReelOffsets = ReelOffsets.ToArray() };

            using var transaction = accessor.StartTransaction();
            transaction[blockIndex, ReelOffsetsOption] = block.ReelOffsets;

            transaction.Commit();
            return true;
        }

        public bool TryGetBlock(IPersistentStorageAccessor accessor, int blockIndex, out ReelControllerOptions block)
        {
            block = new ReelControllerOptions
            {
                ReelOffsets = (int[])accessor[blockIndex, ReelOffsetsOption]
            };

            return true;
        }

        public Task<bool> TiltReels()
        {
            return HandleReelSpinningActionAsync(TiltReelsInternal);
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

            if (!(_stateManager?.Fire(
                    ReelControllerTrigger.Disable,
                    new DisabledEvent(ReelControllerId, ReasonDisabled)) ?? false))
            {
                return;
            }

            Logger.Debug($"Disabling for {ReasonDisabled}");
            Implementation?.Disable()?.WaitForCompletion();
        }

        protected override void Enabling(EnabledReasons reason, DisabledReasons remedied)
        {
            if (_stateManager == null)
            {
                return;
            }

            if (Enabled)
            {
                if (_stateManager.Fire(ReelControllerTrigger.Enable, new EnabledEvent(ReelControllerId, reason)))
                {
                    Implementation?.Enable()?.WaitForCompletion();
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
                    _reelControllerImplementation = null;
                }

                var stateManager = _stateManager;
                _stateManager = null;
                stateManager?.Dispose();
                _reelSpinningLock.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void Initializing()
        {
            _reelControllerImplementation = AddinFactory.CreateAddin<IReelControllerImplementation>(
                DeviceImplementationsExtensionPath,
                ServiceProtocol);

            if (_reelControllerImplementation == null)
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
            _stateManager?.Fire(ReelControllerTrigger.Inspecting);
        }

        protected override void SubscribeToEvents(IEventBus eventBus)
        {
        }

        private async Task<bool> HomeReelsInternal(IReadOnlyDictionary<int, int> reelOffsets)
        {
            Logger.Debug("HomeReels with stops called");

            if (!(_stateManager?.Fire(ReelControllerTrigger.HomeReels) ?? false))
            {
                Logger.Debug("HomeReels - Fire FAILED - CAN NOT HOME");
                return false;
            }

            var allHomed = (await Task.WhenAll(
                    reelOffsets.Select(
                        x =>
                        {
                            Logger.Debug($"homing reel {x.Key} to step {x.Value}");
                            if (!ConnectedReels.Contains(x.Key))
                            {
                                return Task.FromResult(true);
                            }

                            if (!(_stateManager?.Fire(ReelControllerTrigger.HomeReels, x.Key) ?? false))
                            {
                                Logger.Debug($"HomeReels - Fire FAILED for reel {x.Key} - CAN NOT HOME");
                                return Task.FromResult(false);
                            }

                            return Implementation?.HomeReel(x.Key, x.Value) ?? Task.FromResult(false);
                        })))
                .All(x => x);

            if (!allHomed)
            {
                await TiltReelsInternal();
            }

            return allHomed;
        }

        private async Task<bool> TiltReelsInternal()
        {
            _stateManager?.FireAll(ReelControllerTrigger.TiltReels);
            var result = await (Implementation?.TiltReels() ?? Task.FromResult(false));
            return result;
        }

        private void ReelControllerOnReelConnected(object sender, ReelEventArgs e)
        {
            _stateManager?.HandleReelConnected(e);
        }

        private void ReelControllerOnReelDisconnected(object sender, ReelEventArgs e)
        {
            if (!Connected)
            {
                return;
            }

            _stateManager?.HandleReelDisconnected(e);
        }

        private void ReelControllerSpinning(object sender, ReelSpinningEventArgs e)
        {
            Logger.Debug($"ReelControllerSpinning reel {e.ReelId}");
            _stateManager?.Fire(ReelControllerTrigger.SpinReel, e.ReelId);
        }

        private void ReelControllerReelStopped(object sender, ReelEventArgs e)
        {
            _steps[e.ReelId] = e.Step;
            if (!(_stateManager?.FireReelStopped(ReelControllerTrigger.ReelStopped, e.ReelId, e) ?? false))
            {
                Logger.Debug($"ReelControllerReelStopped - FAILED FireReelStopped for reel {e.ReelId}");
            }
        }

        private void ReelControllerInitialized(object sender, EventArgs e)
        {
            if (!(_stateManager?.CanFire(ReelControllerTrigger.Initialized) ?? false))
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

            _supportedCapabilities = ReelCapabilitiesFactory.CreateAll(_reelControllerImplementation, _stateManager)
                .ToDictionary(x => x.Key, x => x.Value);

            InitializeReels().WaitForCompletion();
            SetReelOffsets(_reelOffsets.ToArray());

            _stateManager.Fire(ReelControllerTrigger.Initialized, new InspectedEvent(ReelControllerId));
            if (Enabled)
            {
                Implementation?.Enable()?.WaitForCompletion();
            }
            else
            {
                DisabledDetected();
                Implementation?.Disable()?.WaitForCompletion();
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

            _stateManager?.Fire(ReelControllerTrigger.InspectionFailed, new InspectionFailedEvent(ReelControllerId));
            PostEvent(new DisabledEvent(DisabledReasons.Error));
        }

        private void ReelControllerEnabled(object sender, EventArgs e)
        {
            _stateManager?.Fire(ReelControllerTrigger.Enable);
        }

        private void ReelControllerDisabled(object sender, EventArgs e)
        {
            Logger.Debug("ReelControllerDisabled called");
            _stateManager?.Fire(ReelControllerTrigger.Disable);
        }

        private void ReelControllerDisconnected(object sender, EventArgs e)
        {
            _stateManager?.HandleReelControllerDisconnected(Disable);
        }

        private void ReelControllerConnected(object sender, EventArgs e)
        {
            _stateManager?.Fire(ReelControllerTrigger.Connected, new ConnectedEvent(ReelControllerId));
        }

        private void ReelControllerSlowSpinning(object sender, ReelEventArgs e)
        {
            Logger.Debug($"ReelControllerSlowSpinning {e.ReelId}");
            _stateManager?.HandleReelControllerSlowSpinning(e);
        }

        private void ReelControllerFaultOccurred(object sender, ReelControllerFaultedEventArgs e)
        {
            if (e.Faults == ReelControllerFaults.None || !AddError(e.Faults))
            {
                return;
            }

            Logger.Info($"ReelControllerFaultOccurred - ADDED {e.Faults} from the error list");
            Disable(DisabledReasons.Error);
            PostEvent(new HardwareFaultEvent(e.Faults));
        }

        private void ReelControllerFaultOccurred(object sender, ReelFaultedEventArgs e)
        {
            const ReelFaults perReelHardwareFaults = ReelFaults.ReelStall |
                                                     ReelFaults.ReelTamper |
                                                     ReelFaults.ReelOpticSequenceError |
                                                     ReelFaults.UnknownStop |
                                                     ReelFaults.IdleUnknown;

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
                    continue;
                }

                Logger.Info($"ReelControllerFaultOccurred - ADDED {value} to the error list for reel {e.ReelId}");
                Disable(DisabledReasons.Error);
                PostEvent(
                    (perReelHardwareFaults & value) == value
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

            foreach (var value in e.Faults.GetFlags().Where(x => x != ReelFaults.None))
            {
                if (!ClearError(value))
                {
                    continue;
                }

                Logger.Info($"ReelControllerFaultCleared - REMOVED {value} from the error list for reel {e.ReelId}");
                if (!AnyErrors)
                {
                    Enable(EnabledReasons.Reset);
                }

                PostEvent(new HardwareReelFaultClearEvent(ReelControllerId, value));
            }
        }

        private void ReelControllerFaultCleared(object sender, ReelControllerFaultedEventArgs e)
        {
            if (e.Faults == ReelControllerFaults.None || !ClearError(e.Faults))
            {
                return;
            }

            Logger.Info($"ReelControllerFaultCleared - REMOVED {e.Faults} from the error list");
            if (!AnyErrors)
            {
                Enable(EnabledReasons.Reset);
            }

            PostEvent(new HardwareFaultClearEvent(ReelControllerId, e.Faults));
        }

        private void ReadOrCreateOptions()
        {
            if (!this.GetOrAddBlock(OptionsBlock, out var options, ReelControllerId - 1))
            {
                Logger.Error($"Could not access block {OptionsBlock} {ReelControllerId - 1}");
                return;
            }

            _reelOffsets = options.ReelOffsets;

            Logger.Debug($"Block successfully read {OptionsBlock} {ReelControllerId - 1}");
        }

        private void SetReelOffsets(params int[] offsets)
        {
            Implementation.SetReelOffsets(offsets);
        }

        private Task<T> HandleReelSpinningActionAsync<T>(Func<Task<T>> action) =>
            HandleReelSpinningActionAsync(_ => action(), default);

        private async Task<T> HandleReelSpinningActionAsync<T>(
            Func<CancellationToken, Task<T>> action,
            CancellationToken token)
        {
            await _reelSpinningLock.WaitAsync(token).ConfigureAwait(false);
            try
            {
                return await action(token).ConfigureAwait(false);
            }
            finally
            {
                _reelSpinningLock.Release();
            }
        }
    }
}