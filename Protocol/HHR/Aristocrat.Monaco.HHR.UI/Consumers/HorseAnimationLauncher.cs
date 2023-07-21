namespace Aristocrat.Monaco.Hhr.UI.Consumers
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Windows.Media;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Client.Messages;
    using Localization.Properties;
    using Cabinet.Contracts;
    using Controls;
    using Gaming.Contracts;
    using Hardware.Contracts.Cabinet;
    using Kernel;
    using log4net;
    using Storage.Helpers;
    using ViewModels;
    using Views;
    using Aristocrat.Toolkit.Mvvm.Extensions;

    /// <summary>
    ///     Handler responsible for launching Horse Animation.
    /// </summary>
    public class HorseAnimationLauncher : IGameStartCondition, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly ICabinetDetectionService _cabinetDetectionService;
        private readonly IPrizeInformationEntityHelper _prizeEntityHelper;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IGameStartConditionProvider _gameStartConditions;
        private readonly IGamePlayState _gamePlayState;
        private readonly ISystemDisableManager _disableManager;
        private readonly IGamePlayEntityHelper _gamePlayEntity;

        private VenueRaceCollection _venueRaceCollection;
        private VenueRaceCollectionViewModel _venueRaceCollectionViewModel;
        private readonly DisplayRole _currentDisplay;
        private readonly DisplayRole _expectedTopMostDisplay;
        private DateTime _lastAllowedGameStart;
        private bool _disposed;

        public HorseAnimationLauncher(
            IEventBus eventBus,
            ICabinetDetectionService cabinetDetectionService,
            IPrizeInformationEntityHelper prizeEntityHelper,
            IGameStartConditionProvider gameStartConditions,
            IPropertiesManager properties,
            IGamePlayState gamePlayState,
            ISystemDisableManager disableManager,
            IGamePlayEntityHelper gamePlayEntity)
        {
            _eventBus = eventBus
                ?? throw new ArgumentNullException(nameof(eventBus));
            _cabinetDetectionService = cabinetDetectionService
                ?? throw new ArgumentNullException(nameof(cabinetDetectionService));
            _prizeEntityHelper = prizeEntityHelper
                ?? throw new ArgumentNullException(nameof(prizeEntityHelper));
            _gameStartConditions = gameStartConditions
                ?? throw new ArgumentNullException(nameof(gameStartConditions));
            _propertiesManager = properties
                ?? throw new ArgumentNullException(nameof(properties));
            _gamePlayState = gamePlayState
                ?? throw new ArgumentNullException(nameof(gamePlayState));
            _disableManager = disableManager
                ?? throw new ArgumentNullException(nameof(disableManager));
            _gamePlayEntity = gamePlayEntity
                ?? throw new ArgumentNullException(nameof(gamePlayEntity));

            _eventBus.Subscribe<DisplayConnectionChangedEvent>(this, HandleEvent);

            Execute.OnUIThread(
                () =>
                {
                    Logger.Debug($"Rendering capability tier: {RenderCapability.Tier >> 16}");
                    RaceTrackEntry.SetupHorseImages();
                });

            // The cabinet must be first configured with its intended topmost display connected
            LogDisplayInfo();
            _expectedTopMostDisplay = cabinetDetectionService.GetTopmostDisplay();
            _currentDisplay = GetTargetDisplay();
            Logger.Debug($"RedirectAllowed: {RedirectAllowed}, Expected top most display: {_expectedTopMostDisplay}, currentDisplay: {_currentDisplay}");

            if (_currentDisplay != DisplayRole.Topper)
            {
                _propertiesManager.SetProperty(ApplicationConstants.IsTopperOverlayRedirecting, true);
            }

            // If we are now booting up after a restart to clear the disconnect lockup, we need to remove the
            // Display Disconnect lockup that will be present. Do this only if all the other displays are connected as expected.
            if (RedirectAllowed &&
                IsCabinetValid() &&
                _currentDisplay != DisplayRole.Unknown &&
                _disableManager.CurrentDisableKeys.Contains(ApplicationConstants.DisplayDisconnectedLockupKey))
            {
                Logger.Debug($"Rebooted after disconnecting Topper, clearing disconnect lockup");
                _eventBus.Publish(new ClearDisplayDisconnectedLockupEvent());
            }

            _gameStartConditions.AddGameStartCondition(this);

            // If cabinet is in such a condition that it couldn't get a target display, don't setup the animation window
            if (_currentDisplay != DisplayRole.Unknown)
            {
                SetupAnimationWindow();
            }
            else
            {
                Logger.Debug("Can't setup VenueRaceCollection, top most display is unknown");
            }
        }

        public bool CanGameStart()
        {
            if (_venueRaceCollectionViewModel == null)
            {
                return false;
            }

            // Are the horses on the screen? We don't want to allow game start.
            if (_venueRaceCollectionViewModel.IsAnimationVisible)
            {
                // Has it been more than 10 seconds since we last allowed game start? In that
                // case the horses might be stuck on the screen, so we'll let the game start.
                var timeNow = DateTime.UtcNow;
                if (timeNow - _lastAllowedGameStart < TimeSpan.FromSeconds(10))
                {
                    return false;
                }
            }

            _lastAllowedGameStart = DateTime.UtcNow;
            return true;
        }

        private bool TopperConnected => _cabinetDetectionService.IsDisplayConnected(DisplayRole.Topper);

        private bool TopperExpected => _cabinetDetectionService.IsDisplayExpected(DisplayRole.Topper);

        private bool RedirectAllowed => (bool)_propertiesManager.GetProperty(ApplicationConstants.TopperDisplayDisconnectNoReconfigure, false);

        private DisplayRole GetTargetDisplay()
        {
            // If the Topper disconnect is allowed to be cleared, and this EGM has a Topper, figure out which display
            // to put the horse animation window on. Otherwise just return the top most display
            if (!RedirectAllowed || !TopperExpected)
            {
                Logger.Debug($"Redirecting to top most: {_expectedTopMostDisplay}");
                return _expectedTopMostDisplay;
            }

            // Is the Topper connected (or got reconnected)?
            if (TopperConnected)
            {
                Logger.Debug($"Redirecting to: {DisplayRole.Topper}");
                return DisplayRole.Topper;
            }

            // The Topper is now disconnected. Handle case for cabinet with Topper+Top+Main, where Main is landscape
            if (_cabinetDetectionService.GetDisplayDeviceByItsRole(DisplayRole.Main).IsPortrait())
            {
                Logger.Debug($"Redirecting to: {DisplayRole.Main}");
                return DisplayRole.Main;
            }

            Logger.Debug($"Redirecting to: {DisplayRole.Top}");
            return DisplayRole.Top;
        }

        private bool IsCabinetValid()
        {
            // If redirect is allowed, the cabinet is fine whether the Topper is connected or not
            var topperOk = RedirectAllowed
                ? true
                : _cabinetDetectionService.IsDisplayExpected(DisplayRole.Topper)
                    ? _cabinetDetectionService.IsDisplayConnected(DisplayRole.Topper)
                    : true;

            // Is the display expected? If so, it should be connected, otherwise its of no concern
            var topOk = _cabinetDetectionService.IsDisplayExpected(DisplayRole.Top)
                ? _cabinetDetectionService.IsDisplayConnected(DisplayRole.Top)
                : true;
            var mainOk = _cabinetDetectionService.IsDisplayExpected(DisplayRole.Main)
                ? _cabinetDetectionService.IsDisplayConnected(DisplayRole.Main)
                : true;
            var vbdOk = _cabinetDetectionService.IsDisplayExpected(DisplayRole.VBD)
                ? _cabinetDetectionService.IsDisplayConnected(DisplayRole.VBD)
                : true;

            Logger.Debug($"IsCabinetValid Topper: {topperOk}, Top: {topOk}, Main: {mainOk}, VBD: {vbdOk}");

            return topperOk && topOk && mainOk && vbdOk;
        }

        private void SetupAnimationWindow()
        {
            Logger.Debug($"Adding the horse animation on {_currentDisplay}");

            _venueRaceCollectionViewModel = new VenueRaceCollectionViewModel(
                _eventBus,
                _prizeEntityHelper,
                _propertiesManager,
                _gamePlayState,
                _gamePlayEntity);

            Execute.OnUIThread(
                () =>
                {
                    _venueRaceCollection = new VenueRaceCollection(_venueRaceCollectionViewModel);
                    _eventBus.Publish(
                        new ViewInjectionEvent(
                            _venueRaceCollection,
                            _currentDisplay,
                            ViewInjectionEvent.ViewAction.Add));
                });
        }

        private void HandleEvent(DisplayConnectionChangedEvent evt)
        {
            if (!RedirectAllowed || !TopperExpected)
            {
                return;
            }

            Logger.Debug($"DisplayMonitorStatusChangeEvent currentDisplay: {_currentDisplay}");
            LogDisplayInfo();

            // If the Topper is disconnected, the DisplayMonitor will cause the Display Disconnected lockup to appear. If then the Top and/or VBD are disconnected
            // no new lockup will appear since the Display Disconnected lockup is already set, that lockup can represent multiple display disconnects. If then the
            // Top and VBD are reconnected, that leaves just the Topper disconnected, we need to clear that lockup, and put our own lockup (Topper Disconnected...)
            if (IsCabinetValid())
            {
                // We are using events that will get consumed by the DisplayMonitor class. This way the Display Monitor and this class won't get
                // mixed up with enabled and disabling.
                _eventBus.Publish(new ClearDisplayDisconnectedLockupEvent());
            }
            else
            {
                _eventBus.Publish(new SetDisplayDisconnectedLockupEvent());
            }

            if (evt.Display != DisplayRole.Topper)
                return;

            if ((_currentDisplay == DisplayRole.Topper && evt.IsConnected) ||
                (_currentDisplay != DisplayRole.Topper && !evt.IsConnected))
            {
                _disableManager.Enable(HhrConstants.DisplayConnectionChangedRestartRequiredKey);
                return;
            }

            var connectedString = evt.IsConnected
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ConnectedText)
                : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Disconnected);

            var connectString = evt.IsConnected
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Disconnect)
                : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Reconnect);

            var helpText = Localizer.For(CultureFor.Operator).FormatString(ResourceKeys.RestartRequired, connectString, evt.Display);

            var disableReason = new StringBuilder();
            disableReason.Append(evt.Display);
            disableReason.Append(" ");
            disableReason.Append(connectedString);
            disableReason.Append(" - ");
            disableReason.Append(helpText);

            _disableManager.Disable(
                HhrConstants.DisplayConnectionChangedRestartRequiredKey,
                SystemDisablePriority.Immediate,
                () => disableReason.ToString(),
                true,
                () => helpText);
        }

        private void LogDisplayInfo()
        {
            // Log statuses for any troubleshooting
            var topperExpected = _cabinetDetectionService.IsDisplayExpected(DisplayRole.Topper);
            var topperConnected = _cabinetDetectionService.IsDisplayConnected(DisplayRole.Topper);
            var topExpected = _cabinetDetectionService.IsDisplayExpected(DisplayRole.Top);
            var topConnected = _cabinetDetectionService.IsDisplayConnected(DisplayRole.Top);
            var mainExpected = _cabinetDetectionService.IsDisplayExpected(DisplayRole.Main);
            var mainConnected = _cabinetDetectionService.IsDisplayConnected(DisplayRole.Main);
            var mainOrientation = _cabinetDetectionService.GetDisplayDeviceByItsRole(DisplayRole.Main).IsPortrait() ? "P" : "L";
            var vbdExpected = _cabinetDetectionService.IsDisplayExpected(DisplayRole.VBD);
            var vbdConnected = _cabinetDetectionService.IsDisplayConnected(DisplayRole.VBD);

            Logger.Debug($"Expected/Connected: Topper: {topperExpected}/{topperConnected}, Top: {topExpected}/{topConnected}, Main: {mainExpected}/{mainConnected}/{mainOrientation}, VBD: {vbdExpected}/{vbdConnected}");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
                _gameStartConditions.RemoveGameStartCondition(this);
            }

            _disposed = true;
        }
    }

    public static class DisplayDeviceExtensions
    {
        public static bool IsPortrait(this IDisplayDevice device) => device.WorkingArea.Width < device.WorkingArea.Height;
    }
}
