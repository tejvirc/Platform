namespace Aristocrat.Monaco.Gaming.UI.ViewModels.LobbyClock
{
    using System;
    using System.Reflection;
    using Kernel;
    using log4net;
    using MVVM.ViewModel;

    /// <summary>
    /// ViewModel for TimeClockOverlayWindow
    /// </summary>
    public sealed class LobbyClockViewModel : BaseEntityViewModel
    {

        private new static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool _isVisible;
        private bool _flashingEnabled;
        private readonly ILobbyClockService _lobbyClockService;

        public bool FlashingEnabled
        {
            get => _flashingEnabled;
            set => SetProperty(ref _flashingEnabled, value);
        }

        
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public LobbyClockViewModel() : this(
            ServiceManager.GetInstance().TryGetService<ILobbyClockService>())
        {
        }

        public LobbyClockViewModel(ILobbyClockService lobbyClockService)
        {
            _lobbyClockService = lobbyClockService ?? throw new ArgumentNullException(nameof(lobbyClockService));
            _lobbyClockService.Notify += (_, shouldShow) => FlashingEnabled = shouldShow;
        }

    }
}
