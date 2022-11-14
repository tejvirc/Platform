namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Windows;
    using Application.Contracts;
    using Contracts.Events;
    using Kernel;
    using log4net;
    using MVVM;
    using MVVM.ViewModel;

    /// <summary>
    /// ViewModel for Controlling the flashing behavior of the LobbyClock found in StandardUPITemplate.xaml
    /// </summary>
    public sealed class LobbyClockViewModel : BaseEntityViewModel, IDisposable
    {

        private new static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool _isVisible;
        private bool _flash;
        private bool _isDisposed;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IEventBus _eventBus;

        /// <summary>
        /// Trigger clock flashing behavior
        /// </summary>
        public bool Flash
        {
            get => _flash;
            set
            {
                Logger.Debug($"Flashing Property changed to {value}");
                SetProperty(ref _flash, value);
            }
        }

        /// <summary>
        /// Display lobby clock property in StandardUpiTemplate.xaml
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public LobbyClockViewModel() : this(
            ServiceManager.GetInstance().TryGetService<IPropertiesManager>(),
            ServiceManager.GetInstance().TryGetService<IEventBus>())
        {
        }

        public LobbyClockViewModel(IPropertiesManager propertiesManager, IEventBus eventBus)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _eventBus.Subscribe<LobbyClockFlashChangedEvent>(this, Handler);

            IsVisible = _propertiesManager.GetValue(ApplicationConstants.ClockEnabled, false);

        }

        private void Handler(LobbyClockFlashChangedEvent @event)
        {
            // Trigger Flash Animation
            // If this is not run on UIthread it will not display sometimes.
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    Flash = false;
                    Flash = true;
                });
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _eventBus.UnsubscribeAll(this);

            _isDisposed = true;
        }
    }
}