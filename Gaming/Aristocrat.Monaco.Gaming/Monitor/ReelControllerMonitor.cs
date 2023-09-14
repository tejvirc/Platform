namespace Aristocrat.Monaco.Gaming.Monitor
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
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
    using Hardware.Contracts.Reel.Capabilities;
    using Hardware.Contracts.Reel.ControlData;
    using Hardware.Contracts.Reel.Events;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Vgt.Client12.Application.OperatorMenu;
    using ConnectedEvent = Hardware.Contracts.Reel.Events.ConnectedEvent;
    using DisabledEvent = Hardware.Contracts.Reel.Events.DisabledEvent;
    using DisconnectedEvent = Hardware.Contracts.Reel.Events.DisconnectedEvent;
    using EnabledEvent = Hardware.Contracts.Reel.Events.EnabledEvent;
    using HardwareFaultClearEvent = Hardware.Contracts.Reel.Events.HardwareFaultClearEvent;
    using HardwareFaultEvent = Hardware.Contracts.Reel.Events.HardwareFaultEvent;
    using HardwareReelFaultEvent = Hardware.Contracts.Reel.Events.HardwareReelFaultEvent;
    using InspectedEvent = Hardware.Contracts.Reel.Events.InspectedEvent;
    using InspectionFailedEvent = Hardware.Contracts.Reel.Events.InspectionFailedEvent;
    using ReelControllerState = Hardware.Contracts.Reel.ReelControllerState;

    public class ReelControllerMonitor : GenericBaseMonitor
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private const string ReelsNeedHomingKey = "ReelsNeedHoming";
        private const string ReelsTiltedKey = "ReelsTilted";
        private const string MismatchedKey = "ReelMismatched";
        private const string LoadingAnimationFileKey = "LoadingAnimationFile";
        private const string ReelDeviceName = "ReelController";
        private const string FailedHoming = "FailedHoming";
        private const string LightShowExtension = ".lightshow";

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
            GamingConstants.ReelsTiltedGuid,
            GamingConstants.ReelsNeedHomingGuid
        };

        private readonly IEventBus _eventBus;
        private readonly Lazy<IReelController> _reelController;
        private readonly ISystemDisableManager _disableManager;
        private readonly IGamePlayState _gamePlayState;
        private readonly IGameProvider _gameProvider;
        private readonly IGameService _gameService;
        private readonly IGameHistory _gameHistory;
        private readonly IEdgeLightingController _edgeLightingController;
        private readonly IOperatorMenuLauncher _operatorMenuLauncher;
        private readonly IReelBrightnessCapabilities _brightnessCapability;
        private readonly SemaphoreSlim _tiltLock = new(1, 1);
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly Progress<LoadingAnimationFileModel> _loadingAnimationFileProgress = new();

        private IEdgeLightToken _edgeLightToken;
        private bool _disposed;
        private bool _homeStepsSet;
        private bool _animationFilesAlreadyLoaded;

        public ReelControllerMonitor(
            IEventBus eventBus,
            IMessageDisplay message,
            ISystemDisableManager disable,
            IGamePlayState gamePlayState,
            IGameProvider gameProvider,
            IEdgeLightingController edgeLightingController,
            IGameService gameService,
            IGameHistory gameHistory,
            IOperatorMenuLauncher operatorMenuLauncher)
            : base(message, disable)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _disableManager = disable ?? throw new ArgumentNullException(nameof(disable));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _edgeLightingController =
                edgeLightingController ?? throw new ArgumentNullException(nameof(edgeLightingController));
            _operatorMenuLauncher = operatorMenuLauncher ?? throw new ArgumentNullException(nameof(operatorMenuLauncher));
            _reelController = new Lazy<IReelController>(() => ServiceManager.GetInstance().TryGetService<IReelController>());
            Task.Run(async () => await Initialize(_cancellationTokenSource.Token), _cancellationTokenSource.Token)
                .FireAndForget();

            if (ReelController is not null &&
                ReelController.HasCapability<IReelBrightnessCapabilities>())
            {
                    _brightnessCapability = ReelController.GetCapability<IReelBrightnessCapabilities>();
            }
        }

        public override string DeviceName => ReelDeviceName;

        private bool ReelsShouldTilt
        {
            get
            {
                var inAuditMenu = _operatorMenuLauncher.IsShowing;
                var playEnabled = _gamePlayState.Enabled;

                // If in a game round or needing to recover we only want to review the immediate disables
                var inGameRound = _gamePlayState.InGameRound || _gameHistory.IsRecoveryNeeded;
                var disableKeys = inGameRound
                    ? _disableManager.CurrentImmediateDisableKeys
                    : _disableManager.CurrentDisableKeys;

                var isDisabled = disableKeys.Except(AllowedTiltingDisables).Any();
                var shouldTilt = !inAuditMenu && !playEnabled && isDisabled;

                Logger.Debug($"ReelsShouldTilt: {shouldTilt} --> playEnabled={playEnabled}, inGameRound={inGameRound}, isDisabled={isDisabled}, inAuditMenu={inAuditMenu}");

                return shouldTilt;
            }
        }

        private IReelController ReelController => _reelController.Value;

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
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }

            base.Dispose(disposing);
            _disposed = true;
        }

        private async Task Initialize(CancellationToken token)
        {
            if (ReelController is null)
            {
                var expectedReelCount = _gameProvider.GetMinimumNumberOfMechanicalReels();
                if (expectedReelCount > 0)
                {
                    ReelMismatchDisable(0, expectedReelCount);
                }

                using var serviceWaiter = new ServiceWaiter(_eventBus);
                // ReSharper disable once AccessToDisposedClosure
                using var _ = token.Register(() => serviceWaiter.Dispose());
                serviceWaiter.AddServiceToWaitFor<IReelController>();
                if (!serviceWaiter.WaitForServices())
                {
                    return;
                }
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
                GamingConstants.ReelsTiltedGuid,
                true);
            ManageBinaryCondition(
                ReelsNeedHomingKey,
                DisplayableMessageClassification.HardError,
                DisplayableMessagePriority.Immediate,
                GamingConstants.ReelsNeedHomingGuid,
                true);
            ManageBinaryCondition(
                DisabledKey,
                DisplayableMessageClassification.HardError,
                DisplayableMessagePriority.Immediate,
                GamingConstants.ReelsDisabledGuid,
                true);
            ManageBinaryCondition(
                MismatchedKey,
                DisplayableMessageClassification.HardError,
                DisplayableMessagePriority.Immediate,
                ApplicationConstants.ReelCountMismatchDisableKey,
                true);
            ManageBinaryCondition(
                LoadingAnimationFileKey,
                DisplayableMessageClassification.HardError,
                DisplayableMessagePriority.Immediate,
                ApplicationConstants.ReelLoadingAnimationFilesDisableKey,
                true);
            ManageBinaryCondition(
                FailedHoming,
                DisplayableMessageClassification.HardError,
                DisplayableMessagePriority.Immediate,
                GamingConstants.ReelsFailedHomingGuid,
                true);

            await TiltReels(true);
            SubscribeToEvents();
            await CheckDeviceStatus();
            ValidateReelMismatch();
            GetReelHomeSteps();
            await HandleGameInitializationCompleted();
            await CheckLoadingAnimationFiles();
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

            ReelController.ReelHomeSteps = homeSteps;
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
            _eventBus.Subscribe<ReelDisconnectedEvent>(this, ReelDisconnected);
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
            _eventBus.Subscribe<OpenEvent>(this, HandleDoorOpen);
            _eventBus.Subscribe<ReelStoppedEvent>(this, HandleReelStoppedEvent);
            _eventBus.Subscribe<GameAddedEvent>(this, _ => HandleGameAddedEvent());
            _eventBus.Subscribe<GameProcessExitedEvent>(this, GameProcessExitedUnexpected, evt => evt.Unexpected);
            _eventBus.Subscribe<GameProcessExitedEvent>(this, GameProcessExited);
        }

        private async Task GameProcessExitedUnexpected(GameProcessExitedEvent evt, CancellationToken token)
        {
            var homeReels = !ReelsShouldTilt;
            Logger.Debug($"Titling reels because the game process exited.  Will home immediately after: {homeReels}");

            await TiltReels(true).ConfigureAwait(false);
            if (homeReels)
            {
                await HomeReels().ConfigureAwait(false);
            }
        }

        private async Task GameProcessExited(GameProcessExitedEvent evt, CancellationToken token)
        {
            Logger.Debug($"Game exited, resetting brightness to default");
            if (_brightnessCapability is not null)
            {
                await _brightnessCapability.SetBrightness(_brightnessCapability.DefaultReelBrightness);
            }
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
            var logicalState = ReelController.LogicalState;
            if (_disableManager.CurrentDisableKeys.Contains(GamingConstants.ReelsNeedHomingGuid) ||
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
            if (ReelController is not null &&
                ReelController.LogicalState is not ReelControllerState.Tilted &&
                ReelsShouldTilt)
            {
                await TiltReels(_gamePlayState.InGameRound || _gameHistory.IsRecoveryNeeded);
            }
        }

        private async Task HandleGamePlayDisabledEvent(GamePlayDisabledEvent evt, CancellationToken token)
        {
            if (ReelController is not null &&
                ReelController.LogicalState is not ReelControllerState.Tilted &&
                ReelsShouldTilt)
            {
                await TiltReels(_gamePlayState.InGameRound || _gameHistory.IsRecoveryNeeded);
            }
        }

        private void HandleReelStoppedEvent(ReelStoppedEvent reelStoppedEvent)
        {
            Logger.Debug("HandleReelStoppedEvent");
            if (!reelStoppedEvent.IsReelStoppedFromHoming ||
                !ReelController.ConnectedReels.Contains(reelStoppedEvent.ReelId))
            {
                return;
            }

            if (ReelController.ReelStates.Select(x => x.Value).Any(state => state != ReelLogicalState.IdleAtStop))
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

            if (!ReelsShouldTilt)
            {
                await HomeReels();
            }
        }

        private async Task ReelDisconnected(ReelDisconnectedEvent disconnectedEvent, CancellationToken token)
        {
            await TiltReels(true);
            ValidateReelMismatch();

            if (!ReelsShouldTilt)
            {
                await HomeReels();
            }
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
                    $"Validating Connected Reels count.  Expected={expectedReelCount}, HasCount={ReelController?.ConnectedReels.Count}");
                var connectedReelsCount = ReelController?.ConnectedReels.Count ?? 0;
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

        private async Task CheckLoadingAnimationFiles()
        {
            if (_animationFilesAlreadyLoaded || ReelController == null || !ReelController.HasCapability<IReelAnimationCapabilities>())
            {
                return;
            }

            var animationCapabilities = ReelController?.GetCapability<IReelAnimationCapabilities>();
            var details = _gameProvider.GetGames().FirstOrDefault();
            var gameAnimationFiles = details?.PreloadedAnimationFiles.ToList();

            if (animationCapabilities is null || gameAnimationFiles is null || !gameAnimationFiles.Any())
            {
                return;
            }

            try
            {
                _tiltLock.Wait();
                var animationFilesToLoad = new List<AnimationFile>();
                foreach (var f in gameAnimationFiles)
                {
                    var animationType = Path.GetExtension(f.FilePath).Equals(LightShowExtension, StringComparison.InvariantCulture)
                        ? AnimationType.GameLightShow
                        : AnimationType.GameStepperCurve;
                    animationFilesToLoad.Add(new AnimationFile(f.FilePath, animationType, f.FileIdentifier));
                }

                _loadingAnimationFileProgress.ProgressChanged += HandleLoadingAnimationFileProgress;
                if (await animationCapabilities.LoadAnimationFiles(animationFilesToLoad, _loadingAnimationFileProgress, _cancellationTokenSource.Token))
                {
                    _animationFilesAlreadyLoaded = true;
                }
            }
            finally
            {
                _tiltLock.Release();
            }
        }

        private void HandleLoadingAnimationFileProgress(object sender, LoadingAnimationFileModel loadingAnimationFileModel)
        {
            var loadingAnimationFilesText = string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReelController_LoadingAnimationFile),
                loadingAnimationFileModel.Count + "/" + loadingAnimationFileModel.Total);

            switch (loadingAnimationFileModel.State)
            {
                case LoadingAnimationState.Loading:
                    _disableManager.Disable(
                        ApplicationConstants.ReelLoadingAnimationFilesDisableKey,
                        SystemDisablePriority.Immediate,
                        () => loadingAnimationFilesText,
                        true);
                    break;
                case LoadingAnimationState.Error:
                    var disableText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReelControllerFaults_HardwareError) + " " + loadingAnimationFilesText;
                    _disableManager.Enable(ApplicationConstants.ReelLoadingAnimationFilesDisableKey);
                    _disableManager.Disable(
                        ApplicationConstants.ReelLoadingAnimationFilesErrorKey,
                        SystemDisablePriority.Immediate,
                        () => disableText,
                        true);
                    break;
                case LoadingAnimationState.Completed:
                    _disableManager.Enable(ApplicationConstants.ReelLoadingAnimationFilesDisableKey);
                    _loadingAnimationFileProgress.ProgressChanged -= HandleLoadingAnimationFileProgress;
                    break;
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
            if (ReelController is null or { LogicalState: ReelControllerState.Uninitialized or ReelControllerState.Inspecting or ReelControllerState.Disconnected })
            {
                return;
            }

            // Perform a self test to attempt to clear any hardware faults
            await ReelController.SelfTest(false);
            if (NeedsReelFaultsCleared())
            {
                // Clear all reel controller faults
                await HomeReels();
            }
        }

        private Task HandleDoorOpen(DoorBaseEvent doorBaseEvent, CancellationToken token)
        {
            if (_gamePlayState.InGameRound || !_operatorMenuLauncher.IsShowing || _disableManager.CurrentDisableKeys.Contains(GamingConstants.ReelsTiltedGuid))
            {
                return Task.CompletedTask;
            }

            SetBinary(ReelsNeedHomingKey, true);
            return ReelController.HaltReels();
        }

        private bool NeedsReelFaultsCleared()
        {
            return _disableManager.CurrentDisableKeys.All(
                guid => IsReelFault(guid) ||
                guid == ApplicationConstants.LiveAuthenticationDisableKey ||
                guid == ApplicationConstants.OperatorKeyNotRemovedDisableKey ||
                guid == GamingConstants.ReelsNeedHomingGuid ||
                guid == GamingConstants.ReelsTiltedGuid);
        }

        private static bool IsHomeReelsCondition(IEnumerable<Guid> disableKeys, ReelControllerState logicalState)
        {
            var homeReels = disableKeys.All(guid =>
                 guid == ApplicationConstants.LiveAuthenticationDisableKey ||
                 guid == GamingConstants.ReelsNeedHomingGuid ||
                 guid == GamingConstants.ReelsTiltedGuid);

            homeReels = homeReels && logicalState
                is ReelControllerState.Tilted
                or ReelControllerState.Halted
                or ReelControllerState.IdleUnknown;

            return homeReels;
        }

        private async Task SystemDisableRemoved()
        {
            // On removal of any disable check, if the only remaining fault is the reelsTilted fault
            // then home the reels to attempt to clear any reel faults.
            var inGameRound = _gamePlayState.InGameRound || _gameHistory.IsRecoveryNeeded;
            var disableKeys = inGameRound
                ? _disableManager.CurrentImmediateDisableKeys
                : _disableManager.CurrentDisableKeys;

            if (IsHomeReelsCondition(disableKeys, ReelController.LogicalState))
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
            if (ReelController == null)
            {
                return;
            }

            if (ReelController.ReelControllerFaults != ReelControllerFaults.None)
            {
                _eventBus.Publish(new HardwareFaultEvent(ReelController.ReelControllerFaults));
            }

            if (!ReelController.Connected)
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
            if (ReelController == null)
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

                if (ReelController is null ||
                    ReelController.LogicalState is ReelControllerState.Uninitialized or ReelControllerState.Inspecting or ReelControllerState.Disconnected)
                {
                    Logger.Debug($"TiltReels not able to be executed at this time. State is {ReelController?.LogicalState}");
                    return;
                }

                SetBinary(ReelsTiltedKey, true);
                if (immediate)
                {
                    SetBinary(ReelsNeedHomingKey, true);
                }

                DisableReelLights();
                await ReelController.TiltReels();
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

                if (ReelController is null ||
                    ReelController.LogicalState is ReelControllerState.Uninitialized
                        or ReelControllerState.Inspecting
                        or ReelControllerState.Disconnected
                        or ReelControllerState.Disabled
                    || (ReelController.LogicalState != ReelControllerState.Tilted &&
                        ReelController.LogicalState != ReelControllerState.Halted &&
                        ReelController.LogicalState != ReelControllerState.IdleUnknown &&
                        ReelController.LogicalState != ReelControllerState.IdleAtStops))
                {
                    Logger.Debug($"HomeReels not able to be executed at this time. State is {ReelController?.LogicalState}");
                    return;
                }

                SetBinary(ReelsNeedHomingKey, true);
                if (_gameService.Running)
                {
                    _gameService.ShutdownBegin();
                }

                homed = await ReelController.HomeReels();
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