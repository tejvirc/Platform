namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Kernel;
    using Localization.Properties;

    [CLSCompliant(false)]
    public class ReserveMachineViewModel : OperatorMenuPageViewModelBase
    {
        private readonly IReserveService _reserveService;
        private bool _allowPlayerToReserveMachine;
        private bool _isReserveMachineOptionEnabled;
        private int _reserveMachineDurationSelection;

        public ReserveMachineViewModel()
        {
            var container = ServiceManager.GetInstance().GetService<IContainerService>();
            _reserveService = container.Container?.GetInstance<IReserveService>();
        }

        public bool AllowPlayerToReserveMachine
        {
            get => _allowPlayerToReserveMachine;
            set
            {
                if (_allowPlayerToReserveMachine == value)
                {
                    return;
                }

                SetProperty(
                    ref _allowPlayerToReserveMachine,
                    value,
                    nameof(AllowPlayerToReserveMachine),
                    nameof(IsReserveMachineDurationEnabled));

                PropertiesManager.SetProperty(ApplicationConstants.ReserveServiceEnabled, _allowPlayerToReserveMachine);

                if (!_allowPlayerToReserveMachine &&
                    (bool)PropertiesManager.GetProperty(ApplicationConstants.ReserveServiceLockupPresent, false))
                {
                    _reserveService?.ExitReserveMachine();
                }
            }
        }

        public bool IsReserveMachineOptionEnabled
        {
            get => _isReserveMachineOptionEnabled;
            set
            {
                if (_isReserveMachineOptionEnabled == value)
                {
                    return;
                }

                SetProperty(
                    ref _isReserveMachineOptionEnabled,
                    value,
                    nameof(IsReserveMachineOptionEnabled),
                    nameof(IsReserveMachineDurationEnabled));
            }
        }

        public bool IsReserveMachineDurationEnabled => IsReserveMachineOptionEnabled && AllowPlayerToReserveMachine;

        public Dictionary<string, int> ReserveMachineDuration =>
            new Dictionary<string, int>
            {
                { Localizer.For(CultureFor.Operator).GetString(ResourceKeys.FiveMinutes), 5 },
                { Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SixMinutes), 6 },
                { Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SevenMinutes), 7 },
                { Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EightMinutes), 8 },
                { Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NineMinutes), 9 },
                { Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TenMinutes), 10 }
            };

        public int ReserveMachineDurationSelection
        {
            get => _reserveMachineDurationSelection;
            set
            {
                SetProperty(ref _reserveMachineDurationSelection, value, nameof(ReserveMachineDurationSelection));
                PropertiesManager.SetProperty(
                    ApplicationConstants.ReserveServiceTimeoutInSeconds,
                    (int)TimeSpan.FromMinutes(_reserveMachineDurationSelection).TotalSeconds);
            }
        }

        protected override void OnLoaded()
        {
            AllowPlayerToReserveMachine =
                (bool)PropertiesManager.GetProperty(ApplicationConstants.ReserveServiceEnabled, true);

            IsReserveMachineOptionEnabled = !(bool)PropertiesManager.GetProperty(
                ApplicationConstants.ReserveServiceLockupPresent,
                false);

            ReserveMachineDurationSelection = (int)TimeSpan.FromSeconds(
                (int)PropertiesManager.GetProperty(
                    ApplicationConstants.ReserveServiceTimeoutInSeconds,
                    (int)TimeSpan.FromMinutes(5).TotalSeconds)).TotalMinutes;

            EventBus?.Subscribe<PropertyChangedEvent>(this, HandleEvent);
        }

        protected override void OnUnloaded()
        {
            EventBus?.Unsubscribe<PropertyChangedEvent>(this);
        }

        private void HandleEvent(PropertyChangedEvent @event)
        {
            switch (@event.PropertyName)
            {
                //Reserve Machine option is enabled when there's no Machine Reserved lockup already present
                case ApplicationConstants.ReserveServiceLockupPresent:
                    IsReserveMachineOptionEnabled = !(bool)PropertiesManager.GetProperty(
                        ApplicationConstants.ReserveServiceLockupPresent,
                        false);
                    break;
            }
        }
    }
}