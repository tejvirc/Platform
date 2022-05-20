namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using Accounting.Contracts;
    using Contracts;
    using Contracts.Extensions;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Hardware.Contracts;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;
    using MVVM.Command;
    using OperatorMenu;

    [CLSCompliant(false)]
    public class CreditsInPageViewModel : OperatorMenuPageViewModelBase
    {
        private readonly bool _canOverrideMaxCreditsIn;

        private bool _isDirty;
        private decimal _maxCreditsIn;
        private bool _maxCreditsInEnabled;

        public CreditsInPageViewModel()
        {
            ApplyCommand = new ActionCommand<object>(OnApply, OnCanApply);
            ClearCommand = new ActionCommand<object>(OnClear, OnCanClear);

            // Set whether the operator can override max credit in.
            _canOverrideMaxCreditsIn = GetConfigSetting(
                typeof(OptionsPageViewModel),
                OperatorMenuSetting.OperatorCanOverrideMaxCreditsIn,
                false);
        }

        /// <summary>
        ///     Gets or sets action command that applies the options.
        /// </summary>
        public ActionCommand<object> ApplyCommand { get; set; }

        /// <summary>
        ///     Gets or sets action command that clears the Cash In Limit value.
        /// </summary>
        public ActionCommand<object> ClearCommand { get; set; }

        public bool IsDirty
        {
            get => _isDirty;

            set
            {
                _isDirty = value;
                RaisePropertyChanged(nameof(IsDirty));
                ApplyCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the MaxCreditsIn
        /// </summary>
        public decimal MaxCreditsIn
        {
            get => _maxCreditsIn;

            set
            {
                if (_maxCreditsIn != value)
                {
                    ValidateMaxCreditsIn(value);
                    RaisePropertyChanged(nameof(MaxCreditsIn));
                    IsDirty = true;
                    ApplyCommand.RaiseCanExecuteChanged();
                    ClearCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether MaxCreditsIn is enabled
        /// </summary>
        public bool MaxCreditsInEnabled
        {
            get => _maxCreditsInEnabled;

            set
            {
                _maxCreditsInEnabled = value;
                RaisePropertyChanged(nameof(MaxCreditsInEnabled));
            }
        }

        protected override void OnLoaded()
        {
            MaxCreditsIn = PropertiesManager.GetValue(PropertyKey.MaxCreditsIn, AccountingConstants.DefaultMaxTenderInLimit).MillicentsToDollars();

            RaisePropertyChanged(nameof(MaxCreditsIn));

            IsDirty = false;

            ClearCommand.RaiseCanExecuteChanged();

            // Are we configured to override MaxCreditsIn?
            if (_canOverrideMaxCreditsIn)
            {
                // Yes, enable MaxCreditsIn.
                MaxCreditsInEnabled = true;
                InputEnabled = true;
            }
            else
            {
                // No, disable MaxCreditsIn.
                MaxCreditsInEnabled = false;
                InputEnabled = false;
            }
        }

        private bool OnCanApply(object parameter)
        {
            return !PropertyHasErrors(nameof(MaxCreditsIn)) && IsDirty;
        }

        private void OnApply(object parameter)
        {
            PropertiesManager.SetProperty(
                PropertyKey.MaxCreditsIn,
                _maxCreditsIn.DollarsToMillicents());

            ServiceManager.GetInstance().TryGetService<ICurrencyInValidator>()?.IsValid();
            IsDirty = false;
        }

        private bool OnCanClear(object parameter)
        {
            return MaxCreditsIn > 0;
        }

        private void OnClear(object parameter)
        {
            MaxCreditsIn = 0;
        }

        private void ValidateMaxCreditsIn(decimal maxCreditsIn)
        {
            ClearErrors(nameof(MaxCreditsIn));

            if (maxCreditsIn >= ApplicationConstants.MaxCreditsInMax)
            {
                return;
            }

            if (maxCreditsIn < ApplicationConstants.MaxCreditsInMin)
            {
                SetError(nameof(MaxCreditsIn), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MaxCreditsInInvalid));
            }

            _maxCreditsIn = maxCreditsIn;
        }
    }
}
