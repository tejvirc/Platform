namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Windows.Input;
    using Contracts;
    using Contracts.Localization;
    using Kernel;
    using Monaco.Localization.Properties;
    using MVVM.Command;
    using MVVM.ViewModel;

    [CLSCompliant(false)]
    public class OutOfServiceViewModel : BaseViewModel
    {
        private readonly IEventBus _eventBus;
        private readonly IDisableByOperatorManager _disableByOperatorManager;
        private bool _outOfServiceModeButtonIsEnabled;

        public OutOfServiceViewModel()
        {
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            _disableByOperatorManager = ServiceManager.GetInstance().GetService<IDisableByOperatorManager>();

            // Initially it should always be enabled until it's set by the rule access service.
            OutOfServiceModeButtonIsEnabled = true;

            OutOfServiceModeButtonCommand = new ActionCommand<object>(OutOfServiceModeButtonCommandHandler);
        }

        public string OutOfServiceButtonText => _disableByOperatorManager.DisabledByOperator
            ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EnableEGM)
            : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DisableEGM);

        /// <summary>
        ///     Gets or sets action command that handles Out of Service Mode button click
        /// </summary>
        public ICommand OutOfServiceModeButtonCommand { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether Out of Service button is enabled.
        /// </summary>
        public bool OutOfServiceModeButtonIsEnabled
        {
            get => _outOfServiceModeButtonIsEnabled;
            set => SetProperty(ref _outOfServiceModeButtonIsEnabled, value);
        }

        public void OnLoaded()
        {
            _eventBus.Subscribe<OperatorCultureChangedEvent>(this, OnOperatorCultureChanged);
            RaisePropertyChanged(nameof(OutOfServiceButtonText));
        }

        public void OnUnloaded()
        {
            _eventBus.UnsubscribeAll(this);
        }

        private void OnOperatorCultureChanged(OperatorCultureChangedEvent evt)
        {
            RaisePropertyChanged(nameof(OutOfServiceButtonText));
        }

        private void OutOfServiceModeButtonCommandHandler(object obj)
        {
            if (_disableByOperatorManager.DisabledByOperator)
            {
                _disableByOperatorManager.Enable();
            }
            else
            {
                _disableByOperatorManager.Disable(() => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OutOfService));
            }

            RaisePropertyChanged(nameof(OutOfServiceButtonText));
        }
    }
}
