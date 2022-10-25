namespace Aristocrat.Monaco.Gaming.UI.ViewModels.LobbyClock
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Windows;
    using Application.Contracts;
    using Kernel;
    using log4net;
    using MVVM.ViewModel;

    /// <summary>
    /// ViewModel for LobbyClockOverlayWindow
    /// </summary>
    public sealed class LobbyClockViewModel : BaseEntityViewModel, IDisposable
    {

        private new static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool _isVisible;
        private bool _flashingEnabled;
        private readonly ILobbyClockService _lobbyClockService;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IEventBus _eventBus;

        public bool FlashingEnabled
        {
            get => _flashingEnabled;
            set {
                    Logger.Debug($"Flashing Property changed to {value}");
                    SetProperty(ref _flashingEnabled, value);
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set =>SetProperty(ref _isVisible, value);
        }

        public LobbyClockViewModel() : this(
            ServiceManager.GetInstance().TryGetService<ILobbyClockService>(),
            ServiceManager.GetInstance().TryGetService<IPropertiesManager>(),
            ServiceManager.GetInstance().TryGetService<IEventBus>())
        {
        }

        public LobbyClockViewModel(ILobbyClockService lobbyClockService, IPropertiesManager propertiesManager, IEventBus eventBus)
        {
            _lobbyClockService = lobbyClockService ?? throw new ArgumentNullException(nameof(lobbyClockService));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _lobbyClockService.Notify += EnableFlash;

            IsVisible = _propertiesManager.GetValue(ApplicationConstants.ClockEnabled, false);

        }

        private void EnableFlash(object sender, bool flashEnable)
        {
            FlashingEnabled = flashEnable;
        }

        public void Dispose()
        {
            _lobbyClockService.Notify -= EnableFlash;
            
        }
    }
}
