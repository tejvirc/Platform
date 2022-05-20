namespace Aristocrat.Monaco.Gaming.Monitor
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.Monitors;
    using Common;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.EdgeLighting;
    using Hardware.Contracts.Reel;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Runtime;
    using ConnectedEvent = Hardware.Contracts.Reel.ConnectedEvent;
    using DisabledEvent = Hardware.Contracts.Reel.DisabledEvent;
    using DisconnectedEvent = Hardware.Contracts.Reel.DisconnectedEvent;
    using EnabledEvent = Hardware.Contracts.Reel.EnabledEvent;
    using HardwareFaultClearEvent = Hardware.Contracts.Reel.HardwareFaultClearEvent;
    using HardwareFaultEvent = Hardware.Contracts.Reel.HardwareFaultEvent;
    using HardwareReelFaultEvent = Hardware.Contracts.Reel.HardwareReelFaultEvent;
    using InspectedEvent = Hardware.Contracts.Reel.InspectedEvent;
    using InspectionFailedEvent = Hardware.Contracts.Reel.InspectionFailedEvent;
    using ReelControllerState = Hardware.Contracts.Reel.ReelControllerState;

    public class ReelControllerMonitor : GenericBaseMonitor
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string ReelsTiltedKey = "ReelsTilted";
        private const string MismatchedKey = "ReelMismatched";
        private const string ReelDeviceName = "ReelController";
        private const string FailedHoming = "FailedHoming";

        private static readonly Guid ReelsTiltedGuid = new("{AD46A871-616A-4034-9FB5-962F8DE15E79}");
        private static readonly Guid DisabledGuid = new("{B9029021-106D-419B-961F-1B2799817916}");
        private static readonly Guid FailedHomingGuid = new("{3BD10514-10BA-4A48-826F-41ADFECFD01D}");

        private static readonly PatternParameters DisabledPattern = new SolidColorPatternParameters
        {
            Color = Color.Black,
            Priority = StripPriority.PlatformControlled,
            Strips = new[]
            {
                (int)StripIDs.StepperReel1,
                (int)StripIDs.StepperReel2,
                (int)StripIDs.StepperReel3,
                (int)StripIDs.StepperReel4,
                (int)StripIDs.StepperReel5
            }
        };

        private static readonly IReadOnlyCollection<Guid> AllowedTiltingDisables = new[]
        {
            ApplicationConstants.HandpayPendingDisableKey,
            ApplicationConstants.LiveAuthenticationDisableKey,
            ApplicationConstants.OperatorKeyNotRemovedDisableKey,
            ApplicationConstants.OperatorMenuLauncherDisableGuid,
        };

        private readonly IEventBus _eventBus;
        private readonly IReelController _reelController;
        private readonly ISystemDisableManager _disableManager;
        private readonly IGamePlayState _gamePlayState;
        private readonly IGameProvider _gameProvider;
        private readonly IRuntime _runtime;
        private readonly IEdgeLightingController _edgeLightingController;
        private readonly SemaphoreSlim _tiltLock = new(1, 1);

        private IEdgeLightToken _disableToken;
        private bool _disposed;
        private bool _homeStopsSet;

        public ReelControllerMonitor(
            IEventBus eventBus,
            IMessageDisplay message,
            ISystemDisableManager disable,
            IGamePlayState gamePlayState,
            IGameProvider gameProvider,
            IEdgeLightingController edgeLightingController,
            IRuntime runtime)
            : base(message, disable)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _disableManager = disable ?? throw new ArgumentNullException(nameof(disable));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _edgeLightingController =
                edgeLightingController ?? throw new ArgumentNullException(nameof(edgeLightingController));
            _reelController = ServiceManager.GetInstance().TryGetService<IReelController>();
            Initialize().FireAndForget();
        }

        public override string DeviceName => ReelDeviceName;

        private bool ReelsShouldTilt
        {
            get
            {
                var opKeyNotRemoved = _disableManager.CurrentDisableKeys.Contains(ApplicationConstants.OperatorKeyNotRemovedDisableKey);
                var playEnabled = _gamePlayState.Enabled;

                // If in a game round we only want to review the immediate disables
                var disableKeys = _gamePlayState.InGameRound
                    ? _disableManager.CurrentImmediateDisableKeys
                    : _disableManager.CurrentDisableKeys;

                var isDisabled = disableKeys.Except(AllowedTiltingDisables).Any();
                var shouldTilt = !opKeyNotRemoved && !playEnabled && isDisabled;

                Logger.Debug($"ReelsShouldTilt: {shouldTilt} --> playEnabled={playEnabled}, inGameRound={_gamePlayState.InGameRound}, opKeyNotRemoved={opKeyNotRemoved}, isDisabled={isDisabled}");

                return shouldTilt;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
                _tiltLock.Dispose();
            }

            base.Dispose(disposing);
            _disposed = true;
        }

        private async Task Initialize()
        {
            if (_reelController is null)
            {
                var expectedReelCount = _gameProvider.GetMinimumNumberOfMechanicalReels();
                if (expectedReelCount > 0)
                {
                    ReelMismatchDisable(0, expectedReelCount);
                }

                return;
            }

            foreach (var fault in Enum.GetValues(typeof(ReelControllerFaults)).Cast<ReelControllerFaults>())
            {
                ManageErrorEnum(fault, DisplayableMessagePriority.Immediate, true);
            }

            foreach (var reelFault in Enum.GetValues(typeof(ReelFaults)).Cast<ReelFaults>())
            {
                ManageErrorEnum(reelFault, DisplayableMessagePriority.Immediate, true);
            }

            ManageBinaryCondition(
                DisconnectedKey,
                DisplayableMessageClassification.HardError,
                DisplayableMessagePriority.Immediate,
                ApplicationConstants.ReelControllerDisconnectedGuid,
                true);
            ManageBinaryCondition(
                ReelsTiltedKey,
                DisplayableMessageClassification.HardError,
                DisplayableMessagePriority.Immediate,
                ReelsTiltedGuid,
                true);
            ManageBinaryCondition(
                DisabledKey,
                DisplayableMessageClassification.HardError,
                DisplayableMessagePriority.Immediate,
                DisabledGuid,
                true);
            ManageBinaryCondition(
                MismatchedKey,
                DisplayableMessageClassification.HardError,
                DisplayableMessagePriority.Immediate,
                ApplicationConstants.ReelCountMismatchDisableKey,
                true);
            ManageBinaryCondition(
                FailedHoming,
                DisplayableMessageClassification.HardError,
                DisplayableMessagePriority.Immediate,
                FailedHomingGuid,
                true);

            await TiltReels(TiltReason.Initializing);
            SubscribeToEvents();
            CheckDeviceStatus();
            ValidateReelMismatch();
            GetReelHomeStops();
            await HandleGameInitializationCompleted();
        }

        private void HandleGameAddedEvent()
        {
            ValidateReelMismatch();
            GetReelHomeStops();
        }

        private void GetReelHomeStops()
        {
            if (_homeStopsSet)
            {
                return;
            }

            // this assumes all the games for a cabinet with mechanical reels will have
            // the same home stops. Normally there will only be one game.
            var details = _gameProvider.GetGames().FirstOrDefault();
            var home = details?.MechanicalReelHomeStops;
            if (home is null)
            {
                // in this case the home stops will default to all zeros
                return;
            }

            var homeStops = new Dictionary<int, int>();
            for (var i = 0; i < home.Length; i++)
            {
                homeStops[i + 1] = home[i];
                Logger.Debug($"Set reel {i + 1} home position to {homeStops[i + 1]}");
            }

            _reelController.ReelHomeStops = homeStops;
            _homeStopsSet = true;
        }

        private static bool IsReelFault(Guid guid)
        {
            return IsFaultSet<ReelControllerFaults>(guid) || IsFaultSet<ReelFaults>(guid);
        }

        private static bool IsFaultSet<TEnum>(Guid guid) where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum)).Cast<Enum>()
                .Select(EnumHelper.GetAttribute<ErrorGuidAttribute>)
                .Any(attribute => attribute != null && attribute.Id == guid);
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<SystemDisableAddedEvent>(this, HandleSystemDisableAddedEvent);
            _eventBus.Subscribe<GamePlayDisabledEvent>(this, HandleGamePlayDisabledEvent);

            _eventBus.Subscribe<SystemDisableRemovedEvent>(this, (_, _) => SystemDisableRemoved());
            _eventBus.Subscribe<GamePlayEnabledEvent>(this, (_, _) => SystemDisableRemoved());

            _eventBus.Subscribe<ConnectedEvent>(this, _ => Disconnected(false));
            _eventBus.Subscribe<ReelConnectedEvent>(this, ReelsConnected);
            _eventBus.Subscribe<DisconnectedEvent>(this, _ => Disconnected(true));
            _eventBus.Subscribe<EnabledEvent>(this, _ => SetBinary(DisabledKey, false));
            _eventBus.Subscribe<DisabledEvent>(this, _ => HandleDisableEvent());
            _eventBus.Subscribe<HardwareFaultClearEvent>(this, HandleControllerFault);
            _eventBus.Subscribe<HardwareFaultEvent>(this, HandleControllerFault);
            _eventBus.Subscribe<HardwareReelFaultClearEvent>(this, HandleReelFault);
            _eventBus.Subscribe<HardwareReelFaultEvent>(this, HandleReelFault);
            _eventBus.Subscribe<InspectionFailedEvent>(this, _ => Disconnected(true));
            _eventBus.Subscribe<InspectedEvent>(this, _ => Disconnected(false));
            _eventBus.Subscribe<OperatorMenuEnteredEvent>(this, _ => DisableReelLights());
            _eventBus.Subscribe<GameDiagnosticsStartedEvent>(this, _ => ClearDisableState());
            _eventBus.Subscribe<GameDiagnosticsCompletedEvent>(this, _ => DisableReelLights());
            _eventBus.Subscribe<OperatorMenuExitedEvent>(this, (_, _) => ExitOperatorMenu());
            _eventBus.Subscribe<ClosedEvent>(this, HandleDoorClose);
            _eventBus.Subscribe<ReelStoppedEvent>(this, HandleReelStoppedEvent);
            _eventBus.Subscribe<GameAddedEvent>(this, _ => HandleGameAddedEvent());
        }

        private void DisableReelLights()
        {
            _disableToken ??= _edgeLightingController.AddEdgeLightRenderer(DisabledPattern);
        }

        private async Task HandleSystemDisableAddedEvent(SystemDisableAddedEvent evt, CancellationToken token)
        {
            if (ReelsShouldTilt)
            {
                await TiltReels(TiltReason.SystemDisabled);
            }
        }

        private async Task HandleGamePlayDisabledEvent(GamePlayDisabledEvent evt, CancellationToken token)
        {
            if (ReelsShouldTilt)
            {
                await TiltReels(TiltReason.GamePlayDisabled);
            }
        }

        private void HandleReelStoppedEvent(ReelStoppedEvent reelStoppedEvent)
        {
            Logger.Debug("HandleReelStoppedEvent");
            if (!reelStoppedEvent.IsReelStoppedFromHoming ||
                !_reelController.ConnectedReels.Contains(reelStoppedEvent.ReelId))
            {
                return;
            }

            if (_reelController.ReelStates.Select(x => x.Value).Any(state => state != ReelLogicalState.IdleAtStop))
            {
                return;
            }

            Logger.Debug("HandleReelStoppedEvent all reels are stopped, clearing reels tilt");
            SetBinary(ReelsTiltedKey, false);
            ClearDisableState();
        }

        private async Task HandleGameInitializationCompleted()
        {
            if (!ReelsShouldTilt)
            {
                Logger.Debug("HandleGameInitializationCompleted home reels");
                await HomeReels();
            }
            else
            {
                Logger.Debug("HandleGameInitializationCompleted tilt reels for pending tilt");
                await TiltReels(TiltReason.Initializing);
            }
        }

        private void HandleDisableEvent()
        {
            SetBinary(DisabledKey, true);
        }

        private async Task ReelsConnected(ReelConnectedEvent connectedEvent, CancellationToken token)
        {
            await TiltReels(TiltReason.Initializing);
            ValidateReelMismatch();
        }

        private void ValidateReelMismatch()
        {
            try
            {
                _tiltLock.Wait();
                if (!_gameProvider.GetGames().Any())
                {
                    return;
                }

                var expectedReelCount = _gameProvider.GetMinimumNumberOfMechanicalReels();
                Logger.Debug(
                    $"Validating Connected Reels count.  Expected={expectedReelCount}, HasCount={_reelController?.ConnectedReels.Count}");
                var connectedReelsCount = _reelController?.ConnectedReels.Count ?? 0;
                if (expectedReelCount != connectedReelsCount)
                {
                    ReelMismatchDisable(connectedReelsCount, expectedReelCount);
                }
                else
                {
                    _disableManager.Enable(ApplicationConstants.ReelCountMismatchDisableKey);
                }
            }
            finally
            {
                _tiltLock.Release();
            }
        }

        private void ReelMismatchDisable(int connectedReelsCount, int expectedReelCount)
        {
            _disableManager.Disable(
                ApplicationConstants.ReelCountMismatchDisableKey,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReelController_ReelMismatched),
                true,
                () => Localizer.For(CultureFor.Operator).FormatString(
                    ResourceKeys.ConnectedReelsNeeded,
                    connectedReelsCount,
                    expectedReelCount));
        }

        private async Task HandleDoorClose(DoorBaseEvent doorBaseEvent, CancellationToken token)
        {
            if (doorBaseEvent.LogicalId != (int)DoorLogicalId.Main)
            {
                return;
            }

            if (_reelController is not null &&
                _reelController is not { LogicalState: ReelControllerState.Uninitialized or ReelControllerState.Inspecting or ReelControllerState.Disconnected })
            {
                // Perform a self test to attempt to clear any hardware faults
                await _reelController.SelfTest(false);
            }

            SetBinary(FailedHoming, false);
            if (!AreNonReelFaultsActive())
            {
                // Clear all reel controller faults
                await HomeReels();
            }
        }

        private bool AreNonReelFaultsActive()
        {
            foreach (var guid in _disableManager.CurrentDisableKeys)
            {
                // Ignore guid that can be active during door close processing
                if (guid != ApplicationConstants.MainDoorGuid &&
                    guid != ApplicationConstants.LiveAuthenticationDisableKey &&
                    guid != ReelsTiltedGuid &&
                    !IsReelFault(guid))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task ExitOperatorMenu()
        {
            Logger.Debug("ExitOperatorMenu");
            Logger.Debug($"_disableManager.IsDisabled={_disableManager.IsDisabled}");

            var tiltReels = false;
            if (_disableManager.IsDisabled)
            {
                foreach (var guid in _disableManager.CurrentDisableKeys)
                {
                    Logger.Debug($"checking disable guid = {guid}");
                    // Ignore these faults that can be active during exit operator menu
                    if (guid == ApplicationConstants.LiveAuthenticationDisableKey || guid == ReelsTiltedGuid)
                    {
                        continue;
                    }

                    tiltReels = true;
                    break;
                }
            }

            // If exiting the operator menu and there are any faults then slow spin the reels
            if (tiltReels)
            {
                await TiltReels(TiltReason.SystemDisabled);
            }
            else
            {
                await HomeReels();
            }
        }

        private static bool IsHomeReelsCondition(IEnumerable<Guid> disableKeys)
        {
            var homeReels = disableKeys.All(guid =>
                 guid == ApplicationConstants.LiveAuthenticationDisableKey ||
                 guid == ApplicationConstants.OperatorMenuLauncherDisableGuid ||
                 guid == ApplicationConstants.MainDoorGuid ||
                 guid == ReelsTiltedGuid);

            return homeReels;
        }

        private async Task SystemDisableRemoved()
        {
            // On removal of any disable check, if the only remaining fault is the reelsTilted fault
            // then home the reels to attempt to clear any reel faults.
            var disableKeys = _gamePlayState.InGameRound
                ? _disableManager.CurrentImmediateDisableKeys
                : _disableManager.CurrentDisableKeys;

            var logicalState = _reelController.LogicalState;
            if (IsHomeReelsCondition(disableKeys) && logicalState is ReelControllerState.Tilted or ReelControllerState.IdleUnknown)
            {
                await HomeReels();
            }
        }

        private void Disconnected(bool disconnected, string behavioralDelayKey = null)
        {
            Logger.Debug($"Disconnected, disconnected={disconnected}");
            if (!disconnected)
            {
                ValidateReelMismatch();
            }

            SetBinary(DisconnectedKey, disconnected, behavioralDelayKey);
        }

        private void CheckDeviceStatus()
        {
            Logger.Debug("CheckDeviceStatus");
            if (_reelController == null)
            {
                return;
            }

            if (_reelController.ReelControllerFaults != ReelControllerFaults.None)
            {
                _eventBus.Publish(new HardwareFaultEvent(_reelController.ReelControllerFaults));
            }

            if (!_reelController.Connected)
            {
                Disconnected(true);
            }
        }

        private async Task HandleControllerFault(HardwareFaultClearEvent @event, CancellationToken token)
        {
            Logger.Debug("Handle HardwareFaultClearEvent");

            var faults = @event.Fault;
            foreach (var e in faults.GetFlags().Where(x => x != ReelControllerFaults.None))
            {
                ClearFault(e);
            }

            // If all faults have been cleared then home the reels
            if (!_disableManager.IsDisabled)
            {
                await HomeReels();
            }
        }

        private async Task HandleControllerFault(HardwareFaultEvent @event, CancellationToken token)
        {
            Logger.Debug("Handle HardwareFaultEvent");

            var faults = @event.Fault;

            var tiltReels = false;
            foreach (var e in faults.GetFlags().Where(x => x != ReelControllerFaults.None))
            {
                AddFault(e);
                tiltReels = true;
            }

            SetBinary(ReelsTiltedKey, true);
            if (tiltReels && _disableManager.IsDisabled)
            {
                await TiltReels(TiltReason.SystemDisabled);
            }
        }

        private async Task HandleReelFault(HardwareReelFaultClearEvent @event, CancellationToken token)
        {
            Logger.Debug("HandleReelFault HardwareReelFaultClearEvent");

            var faults = @event.Fault;
            foreach (var e in faults.GetFlags().Where(x => x != ReelFaults.None))
            {
                ClearFault(e);
            }

            // If all faults have been cleared then home the reels
            if (!_disableManager.IsDisabled)
            {
                await HomeReels();
            }
        }

        private async Task HandleReelFault(HardwareReelFaultEvent @event, CancellationToken token)
        {
            Logger.Debug("HandleReelFault HardwareReelFaultEvent");

            var faults = @event.Fault;

            // Ignore reel fault events from reels that are not valid
            if (_reelController == null)
            {
                return;
            }

            SetBinary(ReelsTiltedKey, true);
            var tiltReels = false;
            foreach (var e in faults.GetFlags().Where(x => x != ReelFaults.None))
            {
                AddFault(e);
                tiltReels = true;
            }

            if (tiltReels && _disableManager.IsDisabled)
            {
                await TiltReels(TiltReason.SystemDisabled);
            }
        }

        private void ClearDisableState()
        {
            _edgeLightingController.RemoveEdgeLightRenderer(_disableToken);
            _disableToken = null;
        }

        private async Task TiltReels(TiltReason reason)
        {
            try
            {
                await _tiltLock.WaitAsync();

                if (_reelController is null ||
                    _reelController.LogicalState is ReelControllerState.Uninitialized or ReelControllerState.Inspecting or ReelControllerState.Disconnected)
                {
                    Logger.Debug($"TiltReels not able to be executed at this time. State is {_reelController?.LogicalState}");
                    return;
                }

                // Set a fault for reels tilted and slow spinning which will only clear once a home is complete. This
                // will prevent game play from enabling before reels have finished homing. This tilt should not have
                // an associated displayable message.
                if (reason is TiltReason.Initializing or TiltReason.Faulted)
                {
                    SetBinary(ReelsTiltedKey, true);
                }
                DisableReelLights();

                // kill the runtime so it can recover cleanly after the reel tilt is over
                _runtime.Shutdown();

                await _reelController.TiltReels();
            }
            finally
            {
                _tiltLock.Release();
            }

        }

        private async Task HomeReels()
        {
            bool homed;

            try
            {
                await _tiltLock.WaitAsync();

                if (_reelController is null ||
                    _reelController.LogicalState is ReelControllerState.Uninitialized
                        or ReelControllerState.Inspecting
                        or ReelControllerState.Disconnected
                        or ReelControllerState.Disabled
                    || (_reelController.LogicalState != ReelControllerState.Tilted && _reelController.LogicalState != ReelControllerState.IdleUnknown && _reelController.LogicalState != ReelControllerState.IdleAtStops))
                {
                    ClearDisableState();
                    Logger.Debug($"HomeReels not able to be executed at this time. State is {_reelController?.LogicalState}");
                    return;
                }

                homed = await _reelController.HomeReels();
            }
            finally
            {
                _tiltLock.Release();
            }

            if (!homed)
            {
                SetBinary(FailedHoming, true);
                await TiltReels(TiltReason.Faulted);
            }
        }


        private enum TiltReason
        {
            Initializing,
            SystemDisabled,
            GamePlayDisabled,
            Faulted
        }
    }
}