namespace Aristocrat.Monaco.Sas.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.Contracts.Protocol;
    using Application.UI.ConfigWizard;
    using Aristocrat.Sas.Client;
    using Contracts;
    using Contracts.Events;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Kernel;
    using Localization.Properties;
    using Storage.Models;

    /// <summary>
    ///     A view model through which the settings for SAS features can be configured.
    /// </summary>
    public class SasFeatureViewModel : ConfigWizardViewModelBase
    {
        private readonly decimal _maxAftTransferLimit = ((long)SasConstants.MaxAftTransferAmount).CentsToDollars();
        private readonly decimal _defaultAftTransferLimit;

        private readonly (CashableLockupStrategy CashoutAction, bool configurable) _hostDisabledCashoutAction;

        private readonly Dictionary<CashableLockupStrategy, string> _actionStrings =
            new Dictionary<CashableLockupStrategy, string>
            {
                { CashableLockupStrategy.Allowed, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HostDisableCashoutAllowed) },
                { CashableLockupStrategy.NotAllowed, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HostDisableCashoutDisabled) },
                { CashableLockupStrategy.ForceCashout, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HostDisableCashoutForced) }
            };

        private ProtocolConfiguration _sasProtocolConfiguration;

        private string _selectedValidationItem;
        private string _selectedHandpayModeItem;
        private string _selectedHostDisableCashoutActionItem;

        private bool _isAftSettingsConfigurable;
        private bool _isLegacyBonusingConfigurable;
        private bool _isAftBonusingEnabled;
        private bool _isAftPartialTransfersEnabled;
        private bool _isAftInEnabled;
        private bool _isAftOutEnabled;
        private bool _aftInOutInitialEnabled;
        private bool _isAftWinAmountToHostTransfersEnabled;
        private bool _aftTransferLimitEnabled;
        private bool _aftTransferLimitCheckboxEnabled;
        private bool _isLegacyBonusEnabled;
        private bool _bonusTransferStatusEditable;
        private bool _isRequireLP02OnPowerUpEnabled;

        private decimal _aftTransferLimit;

        private decimal _maxAllowedTransferLimit;

        private int _configChangeNotificationIndex;
        private ConfigNotificationTypes _configChangeNotification;

        private (bool enabled, bool configurable) _egmDisabledOnHostOffline;

        private static bool _wizardPageInitialized;

        /// <summary>
        ///     List of notification types to choose from
        /// </summary>
        public List<string> NotificationTypes => new List<string> { "Always", "ExcludeSAS" };

        /// <summary>
        ///     ctor
        /// </summary>
        public SasFeatureViewModel(bool isWizardPage) : base(isWizardPage)
        {
            var settings = PropertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures());
            var disabled = settings.DisableOnDisconnect;
            var configurable = settings.DisableOnDisconnectConfigurable;

            _configChangeNotification = settings.ConfigNotification;

            _egmDisabledOnHostOffline = (disabled, configurable);

            _defaultAftTransferLimit = settings.TransferLimit.CentsToDollars();

            _hostDisabledCashoutAction = (
                PropertiesManager.GetValue(
                    GamingConstants.LockupBehavior,
                    CashableLockupStrategy.Allowed),
                PropertiesManager.GetValue(
                    GamingConstants.LockupBehaviorConfigurable,
                    false));

            _selectedHostDisableCashoutActionItem = _actionStrings[_hostDisabledCashoutAction.CashoutAction];

            var multiProtocolConfigurationProvider = ServiceManager.GetInstance().GetService<IMultiProtocolConfigurationProvider>();
            SasProtocolConfiguration = multiProtocolConfigurationProvider.MultiProtocolConfiguration
                .FirstOrDefault(x => x.Protocol == CommsProtocol.SAS);
        }

        /// <summary>
        ///     Gets items to add to the Validation Type combobox
        /// </summary>
        public ObservableCollection<string> ValidationItems => new ObservableCollection<string>
        {
            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SecureEnhancedLabel),
            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.System),
            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.None)
        };

        /// <summary>
        ///     Gets or sets the Selected Validation Item from the Validation Type combobox
        /// </summary>
        public string SelectedValidationItem
        {
            get => _selectedValidationItem;
            set => SetProperty(ref _selectedValidationItem, value, nameof(SelectedValidationItem));
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the EGM should
        ///     allow bonus transfer options to be modified or not
        /// </summary>
        public bool BonusTransferStatusEditable
        {
            get => _bonusTransferStatusEditable;
            private set => SetProperty(
                ref _bonusTransferStatusEditable,
                value,
                nameof(BonusTransferStatusEditable),
                nameof(AftBonusTransferStatus));
        }

        /// <summary>
        ///     Gets whether the Legacy Bonus setting is configurable
        /// </summary>
        public bool IsLegacyBonusConfigurable
        {
            get => _isLegacyBonusingConfigurable;
            private set => SetProperty(
                ref _isLegacyBonusingConfigurable,
                value,
                nameof(IsLegacyBonusConfigurable));
        }

        /// <summary>
        ///     Gets whether the AFT settings are configurable
        /// </summary>
        public bool IsAftSettingsConfigurable
        {
            get => _isAftSettingsConfigurable;
            private set => SetProperty(
                ref _isAftSettingsConfigurable,
                value,
                nameof(IsAftSettingsConfigurable));
        }

        /// <summary>
        ///     Gets a value indicating whether the EGM should
        ///     allow aft bonus transfer options to be modified or not
        /// </summary>
        public bool AftBonusTransferStatus => BonusTransferStatusEditable && IsAftInEnabled;

        /// <summary>
        ///     Gets or sets a value indicating whether AFT transfers in are allowed or not
        /// </summary>
        public bool IsAftInEnabled
        {
            get => _isAftInEnabled;
            set
            {
                _aftInOutInitialEnabled = GetAftInOutInitiallyEnabled(_isAftInEnabled, IsAftOutEnabled, value);

                SetProperty(
                    ref _isAftInEnabled,
                    IsAftSettingsConfigurable && value,
                    nameof(IsAftInEnabled),
                    nameof(AftBonusTransferStatus),
                    nameof(AftPartialTransfersCheckboxEnabled));

                SetAftTransferLimitState();

                IsAftBonusingEnabled = false;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether AFT transfers out are allowed or not
        /// </summary>
        public bool IsAftOutEnabled
        {
            get => _isAftOutEnabled;
            set
            {
                _aftInOutInitialEnabled = GetAftInOutInitiallyEnabled(IsAftInEnabled, _isAftOutEnabled, value);

                SetProperty(
                    ref _isAftOutEnabled,
                    IsAftSettingsConfigurable && value,
                    nameof(IsAftOutEnabled),
                    nameof(AftPartialTransfersCheckboxEnabled));

                SetAftTransferLimitState();

                IsAftWinAmountToHostTransfersEnabled = false;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether AFT partial transfers are allowed or not
        /// </summary>
        public bool IsAftPartialTransfersEnabled
        {
            get => _isAftPartialTransfersEnabled;
            set => SetProperty(
                ref _isAftPartialTransfersEnabled,
                IsAftSettingsConfigurable && value,
                nameof(IsAftPartialTransfersEnabled));
        }

        /// <summary>
        ///     Gets whether or not AFT partial transfers can be configured or not.
        /// </summary>
        public bool AftPartialTransfersCheckboxEnabled
        {
            get
            {
                var isEnabled = IsAftInEnabled || IsAftOutEnabled;

                if (!isEnabled && IsAftPartialTransfersEnabled)
                {
                    IsAftPartialTransfersEnabled = false;
                }

                return isEnabled;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether AFT bonus transfers are allowed or not
        /// </summary>
        public bool IsAftBonusingEnabled
        {
            get => _isAftBonusingEnabled;
            set => SetProperty(
                ref _isAftBonusingEnabled,
                IsAftSettingsConfigurable && IsAftInEnabled && value,
                nameof(IsAftBonusingEnabled));
        }

        /// <summary>
        ///     Gets or sets a value indicating whether AFT win amount to host transfers are allowed or not
        /// </summary>
        public bool IsAftWinAmountToHostTransfersEnabled
        {
            get => _isAftWinAmountToHostTransfersEnabled;
            set => SetProperty(
                    ref _isAftWinAmountToHostTransfersEnabled,
                    IsAftSettingsConfigurable && IsAftOutEnabled && value,
                    nameof(IsAftWinAmountToHostTransfersEnabled));
        }

        /// <summary>
        ///     Gets or sets the AFT transfer limit amount.
        /// </summary>
        [CustomValidation(typeof(SasFeatureViewModel), nameof(ValidateAftTransferLimit))]
        public decimal AftTransferLimit
        {
            get => _aftTransferLimit;
            set
            {
                if (_maxAftTransferLimit > value && PreviousAftTransferLimit != value)
                {
                    PreviousAftTransferLimit = _aftTransferLimit;
                }
                SetProperty(ref _aftTransferLimit, value, true);
            }
        }

        /// <summary>
        ///     Gets or sets the previous AFT transfer limit.
        /// </summary>
        public decimal PreviousAftTransferLimit { get; set; }

        /// <summary>
        ///     Gets or sets whether AFT transfer limit checkbox is enabled.
        /// </summary>
        public bool AftTransferLimitCheckboxEnabled
        {
            get => _aftTransferLimitCheckboxEnabled;
            set
            {
                _aftTransferLimitCheckboxEnabled = IsAftSettingsConfigurable && value;
                OnPropertyChanged(nameof(AftTransferLimitCheckboxEnabled));
            }
        }

        /// <summary>
        ///     Gets or sets whether AFT transfer limit checkbox is checked.
        /// </summary>
        public bool AftTransferLimitEnabled
        {
            get => _aftTransferLimitEnabled;
            set
            {
                _aftTransferLimitEnabled = value;

                var aftTransferLimit = GetPropertiesAftTransferLimitToDollars();
                if (_aftTransferLimitEnabled && aftTransferLimit == MaxTransferLimit)
                {
                    aftTransferLimit = PreviousAftTransferLimit;
                }

                AftTransferLimit = _aftTransferLimitEnabled ? aftTransferLimit : MaxTransferLimit;

                OnPropertyChanged(nameof(AftTransferLimitEnabled));
            }
        }

        /// <summary>
        ///     Gets items to add to the Handpay Modes combobox
        /// </summary>
        public ObservableCollection<string> HandpayModeItems =>
            new ObservableCollection<string>
            {
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SecureHandpayReporting),
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LegacyHandpayReporting)
            };

        /// <summary>
        ///     Gets or sets the Selected Handpay Mode Item from the Handpay Mode combobox
        /// </summary>
        public string SelectedHandpayModeItem
        {
            get => _selectedHandpayModeItem;
            set => SetProperty(ref _selectedHandpayModeItem, value, nameof(SelectedHandpayModeItem));
        }

        /// <summary>
        ///     Gets items to add to the Handpay Modes combobox
        /// </summary>
        public ObservableCollection<string> HostDisableCashoutActionItems =>
            new ObservableCollection<string>
            {
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HostDisableCashoutAllowed),
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HostDisableCashoutDisabled),
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HostDisableCashoutForced)
            };

        /// <summary>
        ///     Gets or sets the Host Disable Cashout Action
        /// </summary>
        public string SelectedHostDisableCashoutActionItem
        {
            get => _selectedHostDisableCashoutActionItem;
            set => SetProperty(
                ref _selectedHostDisableCashoutActionItem,
                value,
                nameof(SelectedHostDisableCashoutActionItem));
        }

        /// <summary>
        ///     Gets whether host Disable cashout actions is configurable
        /// </summary>
        public bool HostDisableCashoutActionConfigurable =>
            _hostDisabledCashoutAction.configurable;

        /// <summary>
        ///     Gets or Sets EGM disabled on comms timeout
        /// </summary>
        public bool EgmDisabledOnHostOffline
        {
            get => _egmDisabledOnHostOffline.enabled;
            set => SetProperty(
                ref _egmDisabledOnHostOffline.enabled,
                value,
                nameof(EgmDisabledOnHostOffline));
        }

        /// <summary>
        ///     Gets or Sets EGM disabled on comms timeout configurable.
        /// </summary>
        public bool EgmDisabledOnHostOfflineConfigurable => _egmDisabledOnHostOffline.configurable;

        /// <inheritdoc />
        protected override void SaveChanges()
        {
            OnCommitted();
        }

        /// <summary>
        ///     Gets or sets a value indicating whether Legacy Bonus is allowed or not
        /// </summary>
        public bool IsLegacyBonusEnabled
        {
            get => _isLegacyBonusEnabled;
            set => SetProperty(
                ref _isLegacyBonusEnabled,
                IsLegacyBonusConfigurable && value,
                nameof(IsLegacyBonusEnabled));
        }

        /// <summary>
        ///     Gets or sets a value indicating whether Require LP02 On Power-up is allowed or not
        /// </summary>
        public bool IsRequireLP02OnPowerUpEnabled
        {
            get => _isRequireLP02OnPowerUpEnabled;
            set => SetProperty(
                ref _isRequireLP02OnPowerUpEnabled, value,
                nameof(IsRequireLP02OnPowerUpEnabled));
        }

        /// <summary>
        ///     List of Configuration Notification Types to choose from
        /// </summary>
        public ConfigNotificationTypes ConfigChangeNotification
        {
            get => _configChangeNotification;

            set
            {
                OnPropertyChanged(nameof(ConfigChangeNotification));
                SetProperty(ref _configChangeNotification, value, nameof(ConfigChangeNotification));
            }
        }

        /// <summary>
        ///     Configuration Change Notification index
        /// </summary>
        public int ConfigChangeNotificationIndex
        {
            get => _configChangeNotificationIndex;
            set
            {
                _configChangeNotificationIndex = value;
                OnPropertyChanged(nameof(ConfigChangeNotificationIndex));
            }
        }

        private decimal MaxTransferLimit => _maxAllowedTransferLimit;

        /// <summary>
        ///     Gets and sets the SAS Protocol Configuration
        /// </summary>
        public ProtocolConfiguration SasProtocolConfiguration
        {
            get => _sasProtocolConfiguration;

            private set
            {
                if (value == null)
                {
                    _sasProtocolConfiguration = new ProtocolConfiguration(CommsProtocol.SAS);
                }

                _sasProtocolConfiguration = value;
            }
        }

        /// <inheritdoc />
        protected override void Loaded()
        {
            base.Loaded();

            ClearValidationOnUnload = true;

            var ports = PropertiesManager.GetValue(SasProperties.SasPortAssignments, new PortAssignment());
            var settings = PropertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures());
            IsAftSettingsConfigurable = ports.AftPort != HostId.None;

            _maxAllowedTransferLimit = settings.MaxAllowedTransferLimits.CentsToDollars();
            var aftTransferLimit = CapAmount(settings.TransferLimit.CentsToDollars(), MaxTransferLimit, (amount) =>
            {
                settings.TransferLimit = amount;
                return settings;
            });

            // Assign difference values to trigger property update on UI
            // In a scenario where user decided to go back to Machine Setup page to change currency type, and
            // come back to SAS page, since the amount did not change and it wont trigger property update. We manually do it here.
            AftTransferLimit = -1;
            AftTransferLimit = aftTransferLimit;
            PreviousAftTransferLimit = aftTransferLimit;

            BonusTransferStatusEditable = settings.BonusTransferStatusEditable;

            IsLegacyBonusConfigurable = BonusTransferStatusEditable && ports.LegacyBonusPort != HostId.None;

            IsRequireLP02OnPowerUpEnabled = settings.DisabledOnPowerUp;

            ConfigChangeNotificationIndex = NotificationTypes.IndexOf(ConfigChangeNotification.ToString());

            if (IsWizardPage && !_wizardPageInitialized &&
                PropertiesManager.GetValue(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None) == ImportMachineSettings.None)
            {
                // Default values
                Committed = false;

                SelectedValidationItem = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SecureEnhancedLabel);
                SelectedHandpayModeItem = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SecureHandpayReporting);

                AftTransferLimitEnabled = false;
                IsAftInEnabled = false;
                IsAftOutEnabled = false;
                IsAftPartialTransfersEnabled = false;
                IsAftBonusingEnabled = false;
                IsAftWinAmountToHostTransfersEnabled = false;
                IsLegacyBonusEnabled = false;
            }
            else
            {
                SelectedValidationItem = settings.ValidationType == SasValidationType.SecureEnhanced
                    ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SecureEnhancedLabel)
                    : settings.ValidationType == SasValidationType.System
                        ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.System)
                        : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.None);

                SelectedHandpayModeItem = settings.HandpayReportingType == SasHandpayReportingType.SecureHandpayReporting
                    ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SecureHandpayReporting)
                    : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LegacyHandpayReporting);

                IsAftInEnabled = settings.TransferInAllowed;
                IsAftOutEnabled = settings.TransferOutAllowed;
                IsAftPartialTransfersEnabled = settings.PartialTransferAllowed;
                IsAftBonusingEnabled = settings.AftBonusAllowed;
                IsAftWinAmountToHostTransfersEnabled = settings.WinTransferAllowed;

                // allow the server to override SAS setting
                _isLegacyBonusEnabled = settings.LegacyBonusAllowed;
                OnPropertyChanged(nameof(IsLegacyBonusEnabled));
            }

            CheckNavigation();
        }

        private decimal CapAmount(decimal amount, decimal limit, Func<long, SasFeatures> updater)
        {
            if (amount > limit)
            {
                var features = updater.Invoke(limit.DollarsToCents());
                PropertiesManager.SetProperty(SasProperties.SasFeatureSettings, features);

                return limit;
            }

            return amount;
        }

        private void CheckNavigation()
        {
            if (IsWizardPage)
            {
                var isAftTransferLimitValid = AftTransferLimit.Validate(true, MaxTransferLimit.DollarsToMillicents()) is null
                    || !AftTransferLimitEnabled;

                WizardNavigator.CanNavigateForward = isAftTransferLimitValid;
            }
        }

        /// <summary>
        ///     Sets the properties in the property manager
        /// </summary>
        protected override void OnCommitted()
        {
            if (Committed)
            {
                return;
            }

            var reverseActionStrings = _actionStrings.ToDictionary(x => x.Value, x => x.Key);
            var restartProtocol = IsWizardPage;

            var sasValidationType = GetSasValidationTypeFromItem(_selectedValidationItem);
            var aftTransferLimit = AftTransferLimit.DollarsToCents();

            var handpayReportingType = _selectedHandpayModeItem == Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SecureHandpayReporting)
                ? SasHandpayReportingType.SecureHandpayReporting
                : SasHandpayReportingType.LegacyHandpayReporting;

            var validateHandpays = sasValidationType == SasValidationType.SecureEnhanced;
            PropertiesManager.UpdateProperty(AccountingConstants.ValidateHandpays, validateHandpays);
            if (!validateHandpays)
            {
                PropertiesManager.UpdateProperty(AccountingConstants.EnableReceipts, false);
            }

            var settings = (SasFeatures)PropertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).Clone();
            settings.ValidationType = sasValidationType;
            settings.TransferInAllowed = IsAftInEnabled;
            settings.TransferOutAllowed = IsAftOutEnabled;
            settings.AftBonusAllowed = IsAftBonusingEnabled;
            settings.PartialTransferAllowed = IsAftPartialTransfersEnabled;
            settings.WinTransferAllowed = IsAftWinAmountToHostTransfersEnabled;
            settings.TransferLimit = aftTransferLimit;
            settings.HandpayReportingType = handpayReportingType;
            settings.LegacyBonusAllowed = IsLegacyBonusEnabled;
            settings.DisableOnDisconnect = _egmDisabledOnHostOffline.enabled;
            settings.ConfigNotification = ConfigChangeNotification;
            restartProtocol |= PropertiesManager.UpdateProperty(
                SasProperties.SasFeatureSettings,
                settings,
                new SasFeaturesEqualityComparer());
            restartProtocol |= PropertiesManager.UpdateProperty(
                GamingConstants.LockupBehavior,
                reverseActionStrings[_selectedHostDisableCashoutActionItem]);

            Committed = true;
            base.OnCommitted();
            if (restartProtocol)
            {
                if (ConfigChangeNotification == ConfigNotificationTypes.Always)
                {
                    EventBus.Publish(new OperatorMenuSettingsChangedEvent());
                }

                EventBus.Publish(new RestartProtocolEvent());
            }

            if (IsWizardPage && !_wizardPageInitialized)
            {
                _wizardPageInitialized = true;
            }
        }

        /// <inheritdoc />
        protected override void LoadAutoConfiguration()
        {
            var stringValue = string.Empty;
            var boolValue = false;
            var autoConfigured = true;

            // SAS AFT Configuration

            if (AutoConfigurator.GetValue(SasConstants.AftInEnabled, ref boolValue))
            {
                _isAftInEnabled = boolValue;
            }

            if (AutoConfigurator.GetValue(SasConstants.AftOutEnabled, ref boolValue))
            {
                _isAftOutEnabled = boolValue;
            }

            if (AutoConfigurator.GetValue(SasConstants.AftPartialTransferAllowed, ref boolValue))
            {
                _isAftPartialTransfersEnabled = boolValue;
            }

            if (AutoConfigurator.GetValue(SasConstants.AftBonusAllowed, ref boolValue))
            {
                _isAftBonusingEnabled = boolValue;
            }

            if (AutoConfigurator.GetValue(SasConstants.AftWinToHostAllowed, ref boolValue))
            {
                _isAftWinAmountToHostTransfersEnabled = boolValue;
            }

            if (AutoConfigurator.GetValue(SasConstants.AftTransferLimit, ref stringValue))
            {
                autoConfigured &= long.TryParse(stringValue, out var temp);
                if (autoConfigured)
                {
                    AftTransferLimit = temp.CentsToDollars();
                }
            }

            // SAS Miscellaneous

            if (AutoConfigurator.GetValue(SasConstants.SasValidationType, ref stringValue))
            {
                autoConfigured &= ValidationItems.Contains(stringValue);
                if (autoConfigured)
                {
                    _selectedValidationItem = stringValue;
                }
            }

            if (AutoConfigurator.GetValue(SasConstants.HandpayReportingType, ref stringValue))
            {
                autoConfigured &= HandpayModeItems.Contains(stringValue);
                if (autoConfigured)
                {
                    _selectedHandpayModeItem = stringValue;
                }
            }

            if (AutoConfigurator.GetValue(SasConstants.ConfigChangeNotification, ref stringValue))
            {
                autoConfigured &= Enum.TryParse(stringValue, out ConfigNotificationTypes type);
                if (autoConfigured)
                {
                    _configChangeNotification = type;
                }
            }

            if (AutoConfigurator.GetValue(SasConstants.LegacyBonusEnabled, ref boolValue))
            {
                _isLegacyBonusEnabled = boolValue;
            }

            if (autoConfigured)
            {
                base.LoadAutoConfiguration();
            }
        }

        private void SetAftTransferLimitState()
        {
            AftTransferLimitCheckboxEnabled = IsAftInEnabled || IsAftOutEnabled;

            if (!IsAftInEnabled && !IsAftOutEnabled && AftTransferLimitEnabled)
            {
                AftTransferLimitEnabled = false;
            }
            else
            {
                if (IsLoaded && !AftTransferLimitEnabled && _aftInOutInitialEnabled)
                {
                    AftTransferLimitEnabled = true;

                    AftTransferLimit = _defaultAftTransferLimit;
                    PreviousAftTransferLimit = _defaultAftTransferLimit;
                }
                else if (!IsLoaded)
                {
                    if (PreviousAftTransferLimit == MaxTransferLimit || AftTransferLimit > MaxTransferLimit)
                    {
                        AftTransferLimitEnabled = false;
                    }
                    else if ((!IsWizardPage || IsWizardPage &&
                             PropertiesManager.GetValue(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None) != ImportMachineSettings.None)
                             && !AftTransferLimitEnabled
                             && (IsAftInEnabled != IsAftOutEnabled || !AftTransferLimitCheckboxEnabled && (IsAftInEnabled || IsAftOutEnabled)))
                    {
                        AftTransferLimitEnabled = true;
                    }
                }
            }
        }

        private bool GetAftInOutInitiallyEnabled(bool currentAftIn, bool currentAftOut, bool incomingAftInOutValue)
        {
            return IsLoaded
                && IsAftSettingsConfigurable
                && !currentAftIn
                && !currentAftOut
                && incomingAftInOutValue;
        }

        private decimal GetPropertiesAftTransferLimitToDollars()
        {
            return PropertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).TransferLimit
                .CentsToDollars();
        }

        private static SasValidationType GetSasValidationTypeFromItem(string validationItem)
        {
            if (validationItem == Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SecureEnhancedLabel))
            {
                return SasValidationType.SecureEnhanced;
            }

            if (validationItem == Localizer.For(CultureFor.Operator).GetString(ResourceKeys.System))
            {
                return SasValidationType.System;
            }

            return SasValidationType.None;
        }

        /// <summary>
        /// Validates the AFT Transfer Limit.
        /// </summary>
        /// <param name="aftTransferLimit">The AFT Transfer Limit to validate</param>
        /// <param name="context">The validation context.</param>
        /// <returns>ValidationResult</returns>
        public static ValidationResult ValidateAftTransferLimit(decimal aftTransferLimit, ValidationContext context)
        {
            SasFeatureViewModel instance = (SasFeatureViewModel)context.ObjectInstance;
            var errors = aftTransferLimit.Validate(true, instance.MaxTransferLimit.DollarsToMillicents());

            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }

            return new(errors);

        }
    }
}
