namespace Aristocrat.Monaco.Hhr.UI.Consumers
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Media;
    using Application.Contracts;
    using Cabinet.Contracts;
    using Controls;
    using Gaming.Contracts;
    using Hardware.Contracts.Cabinet;
    using Hhr.Events;
    using Kernel;
    using log4net;
    using MVVM;
    using Storage.Helpers;
    using ViewModels;
    using Views;

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

        private VenueRaceCollection _venueRaceCollection;
        private VenueRaceCollectionViewModel _venueRaceCollectionViewModel;
        private DisplayRole _currentDisplay;
        private readonly DisplayRole _expectedTopMostDisplay;
        private DateTime _lastAllowedGameStart;
        private bool _disposed;


        public HorseAnimationLauncher(
            IEventBus eventBus,
            ICabinetDetectionService cabinetDetectionService,
            IPrizeInformationEntityHelper prizeEntityHelper,
            ISystemDisableManager systemDisableManager,
            IGameStartConditionProvider gameStartConditions,
            IPropertiesManager properties,
            IGamePlayState gamePlayState)
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

            _eventBus.Subscribe<DisplayMonitorStatusChangeEvent>(this, HandleEvent);

            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    Logger.Debug($"Rendering capability tier: {RenderCapability.Tier >> 16}");
                    RaceTrackEntry.SetupHorseImages();
                });

            // The cabinet must be configured with its intended topmost display connected
            _expectedTopMostDisplay = cabinetDetectionService.GetTopmostDisplay();
            _currentDisplay = GetTargetDisplay();
            Logger.Debug($"RedirectAllowed: {RedirectAllowed}, Expected top most display: {_expectedTopMostDisplay}, currentDisplay: {_currentDisplay}");

            // If we are now booting up after a restart to clear the disconnect lockup, we need to remove the
            // Display Disconnect lockup that will be present
            if (RedirectAllowed &&
                _currentDisplay != DisplayRole.Unknown &&
                systemDisableManager.CurrentDisableKeys.Contains(ApplicationConstants.DisplayDisconnectedLockupKey))
            {
                Logger.Debug($"Rebooted after disconnecting Topper, clearing disconnect lockup");
                _eventBus.Publish(new ClearDisplayDisconnectedLockupEvent());
            }

            _gameStartConditions.AddGameStartCondition(this);

            SetupAnimationWindow(_currentDisplay);
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
                Logger.Debug($"TopperExpected: {TopperExpected}");
                return _expectedTopMostDisplay;
            }

            // Is the Topper connected(or got reconnected)? No need to redirect anything if so
            if (TopperConnected)
            {
                Logger.Debug($"Topper is connected");
                return _expectedTopMostDisplay;
            }

            // The Topper is disconnected, handle case for cabinet with Topper+Top+Main, where Main is landscape
            if (_cabinetDetectionService.IsDisplayExpectedAndConnected(DisplayRole.Top) &&
                _cabinetDetectionService.IsDisplayExpectedAndConnected(DisplayRole.Main) &&
                !_cabinetDetectionService.GetDisplayDeviceByItsRole(DisplayRole.Main).IsPortrait())
            {
                Logger.Debug($"Redirecting to: {DisplayRole.Top}");
                return DisplayRole.Top;
            }

            // The Topper is disconnected, handle case for cabinet with Topper+Main, where Main is portrait
            if (_cabinetDetectionService.IsDisplayExpectedAndConnected(DisplayRole.Main) &&
                _cabinetDetectionService.GetDisplayDeviceByItsRole(DisplayRole.Main).IsPortrait())
            {
                Logger.Debug($"Redirecting to: {DisplayRole.Main}");
                return DisplayRole.Main;
            }

            Logger.Debug($"Unable to redirect animation window");
            LogDisplayInfo();

            return DisplayRole.Unknown;
        }

        private void SetupAnimationWindow(DisplayRole targetDisplay)
        {
            _currentDisplay = targetDisplay;

            Logger.Debug($"Adding the horse animation on {_currentDisplay}");

            _venueRaceCollectionViewModel = new VenueRaceCollectionViewModel(
                _eventBus,
                _prizeEntityHelper,
                _propertiesManager,
                _gamePlayState);

            MvvmHelper.ExecuteOnUI(
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

        private void HandleEvent(DisplayMonitorStatusChangeEvent evt)
        {
            Logger.Debug($"DisplayMonitorStatusChangeEvent TopperExpected: {TopperExpected}, TopperConnected: {TopperConnected}, currentDisplay: {_currentDisplay}");

            if (!RedirectAllowed)
            {
                return;
            }

            // If the topper is expected and its now disconnected, and its the current targeted display, then attempt to redirect
            if (TopperExpected && !TopperConnected && _currentDisplay == DisplayRole.Topper)
            {
                var newTargetDisplay = GetTargetDisplay();
                // If we get a valid display role, then _currentDisplay will have a valid role, which will allow the
                // lockup to be cleared when the JP key is toggled
                if (newTargetDisplay != DisplayRole.Unknown)
                {
                    // This event will cause a lockup that can only be cleared by restarting the machine
                    _eventBus.Publish(new DisplayConnectionChangedEvent(DisplayRole.Topper, false));
                    // Clear the generic Display Disconnected lockup so that there arent two disconnected lockups on the screen
                    _eventBus.Publish(new ClearDisplayDisconnectedLockupEvent());
                }
            }

            // If the topper is expected and its now connected, and the current targeted display is NOT the Topper,
            // then redirect back to the Topper
            else if (TopperExpected && TopperConnected && _currentDisplay != DisplayRole.Topper)
            {
                var newTargetDisplay = GetTargetDisplay();
                // This should be the Topper
                if (newTargetDisplay == DisplayRole.Topper)
                {
                    // This event will cause a lockup that can only be cleared by restarting the machine
                    _eventBus.Publish(new DisplayConnectionChangedEvent(DisplayRole.Topper, true));
                }
            }
        }

        private void LogDisplayInfo()
        {
            // Log statuses for any troubleshooting
            foreach (DisplayRole role in Enum.GetValues(typeof(DisplayRole)).Cast<DisplayRole>().Where(d => d != DisplayRole.Unknown))
            {
                Logger.Debug(
                    $"{role}: Expected: {_cabinetDetectionService.IsDisplayExpected(role)}, Connected: {_cabinetDetectionService.IsDisplayConnected(role)}, IsPortrait: {_cabinetDetectionService.GetDisplayDeviceByItsRole(role)?.IsPortrait()}");
            }
        }

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
        public static bool IsPortrait(this IDisplayDevice device)
        {
            return device.WorkingArea.Width < device.WorkingArea.Height;
        }
    }
}