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
    public sealed class LobbyClockViewModel : BaseEntityViewModel
    {

        private new static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool _isVisible;
        private bool _flashingEnabled;
        private readonly ILobbyClockService _lobbyClockService;
        private readonly IPropertiesManager _propertiesManager;

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
            ServiceManager.GetInstance().TryGetService<IPropertiesManager>())
        {
        }

        public LobbyClockViewModel(ILobbyClockService lobbyClockService, IPropertiesManager propertiesManager)
        {
            _lobbyClockService = lobbyClockService ?? throw new ArgumentNullException(nameof(lobbyClockService));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _lobbyClockService.Notify += EnableFlash;
            this.PropertyChanged += Changed;
            IsVisible = _propertiesManager.GetValue(ApplicationConstants.ClockEnabled, false);

        }

        private void Changed(object sender, PropertyChangedEventArgs e)
        {
            Logger.Debug($"ViewModel Class Flashing Property changed to {FlashingEnabled}");
        }
        private void EnableFlash(object sender, bool flashEnable)
        {
            FlashingEnabled = flashEnable;
        }
    }
}
