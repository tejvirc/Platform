namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Accounting.Contracts;
    using CommunityToolkit.Mvvm.Input;
    using Contracts;
    using Contracts.Extensions;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Hardware.Contracts;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;
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
            ApplyCommand = new RelayCommand<object>(OnApply, OnCanApply);
            ClearCommand = new RelayCommand<object>(OnClear, OnCanClear);

            // Set whether the operator can override max credit in.
            _canOverrideMaxCreditsIn = GetConfigSetting(
                typeof(OptionsPageViewModel),
                OperatorMenuSetting.OperatorCanOverrideMaxCreditsIn,
                false);
        }

        /// <summary>
        ///     Gets or sets action command that applies the options.
        /// </summary>
        public RelayCommand<object> ApplyCommand { get; set; }

        /// <summary>
        ///     Gets or sets action command that clears the Cash In Limit value.
        /// </summary>
        public RelayCommand<object> ClearCommand { get; set; }

        public bool IsDirty
        {
            get => _isDirty;

            set
            {
                _isDirty = value;
                OnPropertyChanged(nameof(IsDirty));
                ApplyCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the MaxCreditsIn
        /// </summary>
        [CustomValidation(typeof(CreditsInPageViewModel), nameof(MaxCreditsInValidate))]
        public decimal MaxCreditsIn
        {
            get => _maxCreditsIn;

            set
            {
                if (SetProperty(ref _maxCreditsIn, value, true))
                {
                    IsDirty = true;
                    ApplyCommand.NotifyCanExecuteChanged();
                    ClearCommand.NotifyCanExecuteChanged();
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
                OnPropertyChanged(nameof(MaxCreditsInEnabled));
            }
        }

        protected override void OnLoaded()
        {
            MaxCreditsIn = PropertiesManager.GetValue(PropertyKey.MaxCreditsIn, AccountingConstants.DefaultMaxTenderInLimit).MillicentsToDollars();

            OnPropertyChanged(nameof(MaxCreditsIn));

            IsDirty = false;

            ClearCommand.NotifyCanExecuteChanged();

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

        public static ValidationResult MaxCreditsInValidate(decimal maxCreditsIn, ValidationContext context)
        {
            if (maxCreditsIn < ApplicationConstants.MaxCreditsInMin)
            {
                return new(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MaxCreditsInInvalid));
            }
            return ValidationResult.Success;
        }
    }
}
