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
    using Contracts.Events.OperatorMenu;
    using Hardware.Contracts;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.EdgeLighting;
    using Hardware.Contracts.Reel;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Vgt.Client12.Application.OperatorMenu;
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
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private const string ReelsNeedHomingKey = "ReelsNeedHoming";
        private const string ReelsTiltedKey = "ReelsTilted";
        private const string MismatchedKey = "ReelMismatched";
        private const string ReelDeviceName = "ReelController";
        private const string FailedHoming = "FailedHoming";

        private static readonly Guid ReelsTiltedGuid = new("{AD46A871-616A-4034-9FB5-962F8DE15E79}");
        private static readonly Guid ReelsNeedHomingGuid = new("{9613086D-052A-4FCE-9AA0-B279F8C23993}");
        private static readonly Guid DisabledGuid = new("{B9029021-106D-419B-961F-1B2799817916}");
        private static readonly Guid FailedHomingGuid = new("{3BD10514-10BA-4A48-826F-41ADFECFD01D}");

        private static readonly PatternParameters SolidBlackPattern = new SolidColorPatternParameters
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
            ReelsTiltedGuid,
            ReelsNeedHomingGuid
        };

        private readonly IEventBus _eventBus;
        private readonly IReelController _reelController;
        private readonly ISystemDisableManager _disableManager;
        private readonly IGamePlayState _gamePlayState;
        private readonly IGameProvider _gameProvider;
        private readonly IGameService _gameService;
        private readonly IEdgeLightingController _edgeLightingController;
        private readonly IOperatorMenuLauncher _operatorMenuLauncher;
        private readonly SemaphoreSlim _tiltLock = new(1, 1);

        private IEdgeLightToken _edgeLightToken;
        private bool _disposed;
        private bool _homeStepsSet;

        public ReelControllerMonitor(
            IEventBus eventBus,
            IMessageDisplay message,
            ISystemDisableManager disable,
            IGamePlayState gamePlayState,
            IGameProvider gameProvider,
            IEdgeLightingController edgeLightingController,
            IGameService gameService,
            IOperatorMenuLauncher operatorMenuLauncher)
            : base(message, disable)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _disableManager = disable ?? throw new ArgumentNullException(nameof(disable));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _edgeLightingController =
                edgeLightingController ?? throw new ArgumentNullException(nameof(edgeLightingController));
            _operatorMenuLauncher = operatorMenuLauncher ?? throw new ArgumentNullException(nameof(operatorMenuLauncher));
            _reelController = ServiceManager.GetInstance().TryGetService<IReelController>();
            Initialize().FireAndForget();
        }

        public override string DeviceName => ReelDeviceName;

        private bool ReelsShouldTilt
        {
            get
            {
                var inAuditMenu = _operatorMenuLauncher.IsShowing;
                var playEnabled = _gamePlayState.Enabled;

                // If in a game round we only want to review the immediate disables
                var inGameRound = _gamePlayState.InGameRound;
                var disableKeys = inGameRound
                    ? _disableManager.CurrentImmediateDisableKeys
                    : _disableManager.CurrentDisableKeys;

                var isDisabled = disableKeys.Except(AllowedTiltingDisables).Any();
                var shouldTilt = !inAuditMenu && !playEnabled && isDisabled;

                Logger.Debug($"ReelsShouldTilt: {shouldTilt} --> playEnabled={playEnabled}, inGameRound={inGameRound}, isDisabled={isDisabled}, inAuditMenu={inAuditMenu}");

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
                DisplayableMessagePriority.Normal,
                ReelsTiltedGuid,
                true);
            ManageBinaryCondition(
                ReelsNeedHomingKey,
                DisplayableMessageClassification.HardError,
                DisplayableMessagePriority.Immediate,
                ReelsNeedHomingGuid,
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

            await TiltReels(true);
            SubscribeToEvents();
            await CheckDeviceStatus();
            ValidateReelMismatch();
            GetReelHomeSteps();
            await HandleGameInitializationCompleted();
        }

        private void HandleGameAddedEvent()
        {
            ValidateReelMismatch();
            GetReelHomeSteps();
        }

        private void GetReelHomeSteps()
        {
            if (_homeStepsSet)
            {
                return;
            }

            // this assumes all the games for a cabinet with mechanical reels will have
            // the same home steps. Normally there will only be one game.

            var details = _gameProvider.GetGames().FirstOrDefault();
            var home = details?.MechanicalReelHomeSteps;
            if (home is null)
            {
                // in this case the home steps will default to all zeros

                return;
            }

            var homeSteps = new Dictionary<int, int>();
            for (var i = 0; i < home.Length; i++)
            {
                homeSteps[i + 1] = home[i];
                Logger.Debug($"Set reel {i + 1} home position to {homeSteps[i + 1]}");
            }

            _reelController.ReelHomeSteps = homeSteps;
            _homeStepsSet = true;
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

            _eventBus.Subscribe<ConnectedEvent>(this, (_, _) => Disconnected(false));
            _eventBus.Subscribe<ReelConnectedEvent>(this, ReelsConnected);
            _eventBus.Subscribe<DisconnectedEvent>(this, (_, _) => Disconnected(true));
            _eventBus.Subscribe<EnabledEvent>(this, _ => SetBinary(DisabledKey, false));
            _eventBus.Subscribe<DisabledEvent>(this, _ => HandleDisableEvent());
            _eventBus.Subscribe<HardwareFaultClearEvent>(this, HandleControllerFault);
            _eventBus.Subscribe<HardwareFaultEvent>(this, HandleControllerFault);
            _eventBus.Subscribe<HardwareReelFaultClearEvent>(this, HandleReelFault);
            _eventBus.Subscribe<HardwareReelFaultEvent>(this, HandleReelFault);
            _eventBus.Subscribe<InspectionFailedEvent>(this, (_, _) => Disconnected(true));
            _eventBus.Subscribe<InspectedEvent>(this, (_, _) => Disconnected(false));
            _eventBus.Subscribe<OperatorMenuEnteredEvent>(this, _ => DisableReelLights());
            _eventBus.Subscribe<OperatorMenuExitedEvent>(this, (_, _) => HandleOperatorMenuExited());
            _eventBus.Subscribe<GameDiagnosticsStartedEvent>(this, _ => ClearEdgeLightRenderer());
            _eventBus.Subscribe<GameDiagnosticsCompletedEvent>(this, _ => DisableReelLights());
            _eventBus.Subscribe<GameHistoryPageLoadedEvent>(this, HandleGameHistoryPageLoaded);
            _eventBus.Subscribe<ClosedEvent>(this, HandleDoorClose, e => e.LogicalId is (int)DoorLogicalId.Main);
            _eventBus.Subscribe<ReelStoppedEvent>(this, HandleReelStoppedEvent);
            _eventBus.Subscribe<GameAddedEvent>(this, _ => HandleGameAddedEvent());
        }

        private async Task HandleOperatorMenuExited()
        {
            ClearEdgeLightRenderer();

            if (!ReelsShouldTilt)
            {
                return;
            }

            Logger.Debug("HandleOperatorMenuExited tilt reels");
            await TiltReels(true);
        }

        private async Task HandleGameHistoryPageLoaded(GameHistoryPageLoadedEvent evt, CancellationToken token)
        {
            var logicalState = _reelController.LogicalState;
            if (_disableManager.CurrentDisableKeys.Contains(ReelsNeedHomingGuid) ||
                logicalState is ReelControllerState.Tilted or ReelControllerState.IdleUnknown)
            {
                await HomeReels();
            }
        }

        private void DisableReelLights()
        {
            _edgeLightToken ??= _edgeLightingController.AddEdgeLightRenderer(SolidBlackPattern);
        }

        private async Task HandleSystemDisableAddedEvent(SystemDisableAddedEvent evt, CancellationToken token)
        {
            if (_reelController is not null &&
                _reelController.LogicalState is not ReelControllerState.Tilted &&
                ReelsShouldTilt)
            {
                await TiltReels(_gamePlayState.InGameRound);
            }
        }

        private async Task HandleGamePlayDisabledEvent(GamePlayDisabledEvent evt, CancellationToken token)
        {
            if (_reelController is not null &&
                _reelController.LogicalState is not ReelControllerState.Tilted &&
                ReelsShouldTilt)
            {
                await TiltReels(_gamePlayState.InGameRound);
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
            SetBinary(ReelsNeedHomingKey, false);
            ClearEdgeLightRenderer();
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
                await TiltReels();
            }
        }

        private void HandleDisableEvent()
        {
            SetBinary(DisabledKey, true);
        }

        private async Task ReelsConnected(ReelConnectedEvent connectedEvent, CancellationToken token)
        {
            await TiltReels(true);
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
            SetBinary(FailedHoming, false);
            if (_reelController is null or { LogicalState: ReelControllerState.Uninitialized or ReelControllerState.Inspecting or ReelControllerState.Disconnected })
            {
                return;
            }

            // Perform a self test to attempt to clear any hardware faults
            await _reelController.SelfTest(false);
            if (HasReelFaults())
            {
                // Clear all reel controller faults
                await HomeReels();
            }
        }

        private bool HasReelFaults()
        {
            return _disableManager.CurrentDisableKeys.All(
                guid => IsReelFault(guid) || guid == ApplicationConstants.LiveAuthenticationDisableKey ||
                        guid == ReelsTiltedGuid);
        }

        private static bool IsHomeReelsCondition(IEnumerable<Guid> disableKeys)
        {
            var homeReels = disableKeys.All(guid =>
                 guid == ApplicationConstants.LiveAuthenticationDisableKey ||
                 guid == ApplicationConstants.OperatorMenuLauncherDisableGuid ||
                 guid == ReelsNeedHomingGuid ||
                 guid == ReelsTiltedGuid);

            return homeReels;
        }

        private async Task SystemDisableRemoved()
        {
            // On removal of any disable check, if the only remaining fault is the reelsTilted fault
            // then home the reels to attempt to clear any reel faults.
            var inGameRound = _gamePlayState.InGameRound;
            var disableKeys = inGameRound
                ? _disableManager.CurrentImmediateDisableKeys
                : _disableManager.CurrentDisableKeys;

            var logicalState = _reelController.LogicalState;
            if (IsHomeReelsCondition(disableKeys) &&
                logicalState is ReelControllerState.Tilted or ReelControllerState.IdleUnknown)
            {
                await HomeReels();
            }
        }

        private async Task Disconnected(bool disconnected, string behavioralDelayKey = null)
        {
            Logger.Debug($"Disconnected, disconnected={disconnected}");
            if (!disconnected)
            {
                ValidateReelMismatch();
                await TiltReels(true);
            }

            SetBinary(DisconnectedKey, disconnected, behavioralDelayKey);
        }

        private async Task CheckDeviceStatus()
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
                await Disconnected(true);
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
                await TiltReels(true);
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
                await TiltReels(true);
            }
        }

        private void ClearEdgeLightRenderer()
        {
            if (_edgeLightToken != null)
            {
                _edgeLightingController.RemoveEdgeLightRenderer(_edgeLightToken);
                _edgeLightToken = null;
            }
        }

        private async Task TiltReels(bool immediate = false)
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

                SetBinary(ReelsTiltedKey, true);
                if (immediate)
                {
                    SetBinary(ReelsNeedHomingKey, true);
                }

                DisableReelLights();
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
                    Logger.Debug($"HomeReels not able to be executed at this time. State is {_reelController?.LogicalState}");
                    return;
                }

                SetBinary(ReelsNeedHomingKey, true);
                if (_gameService.Running)
                {
                    _gameService.ShutdownBegin();
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
                await TiltReels(true);
            }
        }
    }
}