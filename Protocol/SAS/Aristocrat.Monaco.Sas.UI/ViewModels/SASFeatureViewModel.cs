namespace Aristocrat.Monaco.Sas.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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

        private bool _isFundTransferSettingsConfigurable;
        private bool _isLegacyBonusingConfigurable;
        private bool _isAftBonusingEnabled;
        private bool _isPartialTransfersEnabled;
        private bool _isTransferInEnabled;
        private bool _isTransferOutEnabled;
        private bool _transferInOutInitialEnabled;
        private bool _isAftWinAmountToHostTransfersEnabled;
        private bool _transferLimitEnabled;
        private bool _transferLimitCheckboxEnabled;
        private bool _isLegacyBonusEnabled;
        private bool _bonusTransferStatusEditable;
        private bool _isRequireLP02OnPowerUpEnabled;

        private decimal _transferLimit;
        private decimal _creditLimit;
        private decimal _maxCreditLimit;

        private decimal _maxAllowedTransferLimit;

        private int _configChangeNotificationIndex;
        private ConfigNotificationTypes _configChangeNotification;

        private (bool enabled, bool configurable) _egmDisabledOnHostOffline;
        private bool _isAft;

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
        /// 	Check if Aft is enabled for fund transfer.
        /// </summary>
        public bool IsAft
        {
            get => _isAft;
            set
            {
                _isAft = value;
                RaisePropertyChanged(nameof(TransferInLabelResourceKey), nameof(TransferOutLabelResourceKey), nameof(FundTransferTitleLabelResourceKey));
            }
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
        ///     Gets whether the AFT/EFT settings are configurable
        /// </summary>
        public bool IsFundTransferSettingsConfigurable
        {
            get => _isFundTransferSettingsConfigurable;
            private set => SetProperty(
                ref _isFundTransferSettingsConfigurable,
                value,
                nameof(IsFundTransferSettingsConfigurable),
                nameof(IsFundTransferSettingsVisible));
        }

        /// <summary>
        ///     Gets a value indicating whether the EGM should
        ///     allow aft bonus transfer options to be modified or not
        /// </summary>
        public bool AftBonusTransferStatus => BonusTransferStatusEditable && IsTransferInEnabled;

        /// <summary>
        ///     Gets a value indicating if Aft/EFT settings UI is visible to the operator.
        /// </summary>
        public bool IsFundTransferSettingsVisible => IsFundTransferSettingsConfigurable && SasProtocolConfiguration.IsFundTransferHandled;

        /// <summary>
        ///     Gets or sets a value indicating whether AFT/EFT transfers in are allowed or not
        /// </summary>
        public bool IsTransferInEnabled
        {
            get => _isTransferInEnabled;
            set
            {
                _transferInOutInitialEnabled = GetTransferInOutInitiallyEnabled(_isTransferInEnabled, IsTransferOutEnabled, value);

                SetProperty(
                    ref _isTransferInEnabled,
                    IsFundTransferSettingsConfigurable && value,
                    nameof(IsTransferInEnabled),
                    nameof(AftBonusTransferStatus),
                    nameof(PartialTransfersCheckboxEnabled));

                SetAftTransferLimitState();

                IsAftBonusingEnabled = false;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether AFT/EFT transfers out are allowed or not
        /// </summary>
        public bool IsTransferOutEnabled
        {
            get => _isTransferOutEnabled;
            set
            {
                _transferInOutInitialEnabled = GetTransferInOutInitiallyEnabled(IsTransferInEnabled, _isTransferOutEnabled, value);

                SetProperty(
                    ref _isTransferOutEnabled,
                    IsFundTransferSettingsConfigurable && value,
                    nameof(IsTransferOutEnabled),
                    nameof(PartialTransfersCheckboxEnabled));

                SetAftTransferLimitState();

                IsAftWinAmountToHostTransfersEnabled = false;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether AFT/EFT partial transfers are allowed or not
        /// </summary>
        public bool IsPartialTransfersEnabled
        {
            get => _isPartialTransfersEnabled;
            set => SetProperty(
                ref _isPartialTransfersEnabled,
                IsFundTransferSettingsConfigurable && value,
                nameof(IsPartialTransfersEnabled));
        }

        /// <summary>
        ///     Gets whether or not AFT/Eft partial transfers can be configured or not.
        /// </summary>
        public bool PartialTransfersCheckboxEnabled
        {
            get
            {
                var isEnabled = IsTransferInEnabled || IsTransferOutEnabled;

                if (!isEnabled && IsPartialTransfersEnabled)
                {
                    IsPartialTransfersEnabled = false;
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
                IsFundTransferSettingsConfigurable && IsTransferInEnabled && value,
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
                    IsFundTransferSettingsConfigurable && IsTransferOutEnabled && value,
                    nameof(IsAftWinAmountToHostTransfersEnabled));
        }

        /// <summary>
        ///     Resource key of fund transfer in label
        /// </summary>
        public string TransferInLabelResourceKey => IsAft ? ResourceKeys.AftInLabel : ResourceKeys.EftInLabel;

        /// <summary>
        ///     Resource key of fund transfer title label
        /// </summary>
        public string FundTransferTitleLabelResourceKey => IsAft ? ResourceKeys.AFTTitleLabel : ResourceKeys.EFTTitleLabel;

        /// <summary>
        ///     Resource key of transfer out label
        /// </summary>
        public string TransferOutLabelResourceKey => IsAft ? ResourceKeys.AftOutLabel : ResourceKeys.EftOutLabel;

        /// <summary>
        ///     Gets or sets the AFT/EFT transfer limit amount.
        /// </summary>
        public decimal TransferLimit
        {
            get => _transferLimit;
            set
            {
                if ((_maxAftTransferLimit > value || IsCreditLimitMaxed) && PreviousTransferLimit != value)
                {
                    PreviousTransferLimit = _transferLimit;
                }

                if (SetProperty(ref _transferLimit, value, nameof(TransferLimit)))
                {
                    SetError(
                        nameof(TransferLimit),
                        _transferLimit.Validate(true, MaxTransferLimit.DollarsToMillicents()));
                }

                RaisePropertyChanged(nameof(TransferLimit));
            }
        }

        /// <summary>
        ///     Gets or sets the previous AFT/EFT transfer limit.
        /// </summary>
        public decimal PreviousTransferLimit { get; set; }

        /// <summary>
        ///     Gets or sets whether AFT/EFT transfer limit checkbox is enabled.
        /// </summary>
        public bool TransferLimitCheckboxEnabled
        {
            get => _transferLimitCheckboxEnabled;
            set
            {
                _transferLimitCheckboxEnabled = IsFundTransferSettingsConfigurable && value;
                RaisePropertyChanged(nameof(TransferLimitCheckboxEnabled));
            }
        }

        /// <summary>
        ///     Gets or sets whether AFT/EFT transfer limit checkbox is checked.
        /// </summary>
        public bool TransferLimitEnabled
        {
            get => _transferLimitEnabled;
            set
            {
                _transferLimitEnabled = value;

                var aftTransferLimit = GetPropertiesTransferLimitToDollars();
                if (_transferLimitEnabled && (aftTransferLimit == MaxTransferLimit || IsCreditLimitMaxed))
                {
                    aftTransferLimit = PreviousTransferLimit;
                }

                TransferLimit = _transferLimitEnabled ? aftTransferLimit : MaxTransferLimit;

                RaisePropertyChanged(nameof(TransferLimitEnabled));
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
                RaisePropertyChanged(nameof(ConfigChangeNotification));
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
                RaisePropertyChanged(nameof(ConfigChangeNotificationIndex));
            }
        }

        private bool IsCreditLimitMaxed => _creditLimit == _maxCreditLimit;

        private decimal MaxTransferLimit =>
            _maxAllowedTransferLimit < _creditLimit ? _maxAllowedTransferLimit : _creditLimit;

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
            IsAft = ports.FundTransferType == FundTransferType.Aft;
            IsFundTransferSettingsConfigurable = ports.FundTransferPort != HostId.None;

            _creditLimit = PropertiesManager
                .GetValue(AccountingConstants.MaxCreditMeter, long.MaxValue)
                .MillicentsToDollars();

            _maxCreditLimit = PropertiesManager
                .GetValue(AccountingConstants.MaxCreditMeterMaxAllowed, long.MaxValue)
                .MillicentsToDollars();

            _maxAllowedTransferLimit = settings.MaxAllowedTransferLimits.CentsToDollars();
            var transferLimit = CapAmount(settings.TransferLimit.CentsToDollars(), MaxTransferLimit, (amount) =>
            {
                settings.TransferLimit = amount;
                return settings;
            });

            // Assign difference values to trigger property update on UI
            // In a scenario where user decided to go back to Machine Setup page to change currency type, and
            // come back to SAS page, since the amount did not change and it wont trigger property update. We manually do it here.
            TransferLimit = -1;
            TransferLimit = transferLimit;
            PreviousTransferLimit = transferLimit;

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

                TransferLimitEnabled = false;
                IsTransferInEnabled = false;
                IsTransferOutEnabled = false;
                IsPartialTransfersEnabled = false;
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

                IsTransferInEnabled = settings.TransferInAllowed;
                IsTransferOutEnabled = settings.TransferOutAllowed;
                IsPartialTransfersEnabled = settings.PartialTransferAllowed;
                IsAftBonusingEnabled = settings.AftBonusAllowed;
                IsAftWinAmountToHostTransfersEnabled = settings.WinTransferAllowed;

                // allow the server to override SAS setting
                _isLegacyBonusEnabled = settings.LegacyBonusAllowed;
                RaisePropertyChanged(nameof(IsLegacyBonusEnabled));
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
                var isTransferLimitValid = TransferLimit.Validate(true, MaxTransferLimit.DollarsToMillicents()) is null
                    || !TransferLimitEnabled;

                WizardNavigator.CanNavigateForward = isTransferLimitValid;
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
            var transferLimit = TransferLimit.DollarsToCents();

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
            settings.TransferInAllowed = IsTransferInEnabled;
            settings.TransferOutAllowed = IsTransferOutEnabled;
            settings.FundTransferType = IsAft ? FundTransferType.Aft : FundTransferType.Eft;
            settings.AftBonusAllowed = IsAftBonusingEnabled;
            settings.PartialTransferAllowed = IsPartialTransfersEnabled;
            settings.WinTransferAllowed = IsAftWinAmountToHostTransfersEnabled;
            settings.TransferLimit = transferLimit;
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
            if (AutoConfigurator.GetValue("SasValidationType", ref stringValue))
            {
                autoConfigured &= ValidationItems.Contains(stringValue);
                if (autoConfigured)
                {
                    _selectedValidationItem = stringValue;
                }
            }

            if (AutoConfigurator.GetValue("TransferInEnabled", ref boolValue))
            {
                _isTransferInEnabled = boolValue;
            }

            if (AutoConfigurator.GetValue("TransferOutEnabled", ref boolValue))
            {
                _isTransferOutEnabled = boolValue;
            }

            if (AutoConfigurator.GetValue("AftBonusEnabled", ref boolValue))
            {
                _isAftBonusingEnabled = boolValue;
            }

            if (AutoConfigurator.GetValue("PartialTransferEnabled", ref boolValue))
            {
                _isPartialTransfersEnabled = boolValue;
            }

            if (AutoConfigurator.GetValue("AftWinAmountToHostEnabled", ref boolValue))
            {
                _isAftWinAmountToHostTransfersEnabled = boolValue;
            }

            if (AutoConfigurator.GetValue("LegacyBonusEnabled", ref boolValue))
            {
                _isLegacyBonusEnabled = boolValue;
            }

            if (AutoConfigurator.GetValue("TransferLimit", ref stringValue))
            {
                autoConfigured &= long.TryParse(stringValue, out var temp);
                if (autoConfigured)
                {
                    TransferLimit = temp.CentsToDollars();
                }
            }

            if (AutoConfigurator.GetValue("SasValidationType", ref stringValue))
            {
                autoConfigured &= ValidationItems.Contains(stringValue);
                if (autoConfigured)
                {
                    _selectedValidationItem = stringValue;
                }
            }

            if (AutoConfigurator.GetValue("HandpayReportingType", ref stringValue))
            {
                autoConfigured &= HandpayModeItems.Contains(stringValue);
                if (autoConfigured)
                {
                    _selectedHandpayModeItem = stringValue;
                }
            }

            if (AutoConfigurator.GetValue("ChangeNotification", ref stringValue))
            {
                autoConfigured &= NotificationTypes.Contains(stringValue);
                if (autoConfigured)
                {
                    switch (stringValue)
                    {
                        case "Always":
                            ConfigChangeNotification = ConfigNotificationTypes.Always;
                            break;
                        case "ExcludeSAS":
                            ConfigChangeNotification = ConfigNotificationTypes.ExcludeSAS;
                            break;
                    }
                }
            }

            if (AutoConfigurator.GetValue("LegacyBonusEnabled", ref boolValue))
            {
                _isLegacyBonusEnabled = boolValue;
            }

            if (AutoConfigurator.GetValue("LP02OnPowerUp", ref boolValue))
            {
                IsRequireLP02OnPowerUpEnabled = boolValue;
            }

            if (AutoConfigurator.GetValue("DisableOnCommunicationsLost", ref boolValue))
            {
                EgmDisabledOnHostOffline = boolValue;
            }

            if (autoConfigured)
            {
                base.LoadAutoConfiguration();
            }
        }

        private void SetAftTransferLimitState()
        {
            TransferLimitCheckboxEnabled = IsCreditLimitMaxed && (IsTransferInEnabled || IsTransferOutEnabled);

            if (!IsTransferInEnabled && !IsTransferOutEnabled && TransferLimitEnabled)
            {
                TransferLimitEnabled = false;
            }
            else
            {
                if (IsLoaded && !TransferLimitEnabled && _transferInOutInitialEnabled)
                {
                    TransferLimitEnabled = true;

                    var limit = _creditLimit <= _defaultAftTransferLimit
                        ? _creditLimit
                        : _defaultAftTransferLimit;

                    TransferLimit = limit;
                    PreviousTransferLimit = limit;
                }
                else if (!IsLoaded)
                {
                    if ((PreviousTransferLimit == MaxTransferLimit && IsCreditLimitMaxed) || TransferLimit > MaxTransferLimit)
                    {
                        TransferLimitEnabled = false;
                    }
                    else if ((!IsWizardPage || IsWizardPage &&
                             PropertiesManager.GetValue(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None) != ImportMachineSettings.None)
                             && !TransferLimitEnabled
                             && (IsTransferInEnabled != IsTransferOutEnabled || (!TransferLimitCheckboxEnabled && (IsTransferInEnabled || IsTransferOutEnabled))))
                    {
                        TransferLimitEnabled = true;
                    }
                }
            }
        }

        private bool GetTransferInOutInitiallyEnabled(bool currentAftIn, bool currentAftOut, bool incomingAftInOutValue)
        {
            return IsLoaded
                && IsFundTransferSettingsConfigurable
                && !currentAftIn
                && !currentAftOut
                && incomingAftInOutValue;
        }

        private decimal GetPropertiesTransferLimitToDollars()
        {
            return PropertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).TransferLimit.CentsToDollars();
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

        /// <inheritdoc />
        protected override void SetError(string propertyName, string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                ClearErrors(propertyName);
            }
            else
            {
                base.SetError(propertyName, error);
            }

            CheckNavigation();
        }
    }
}
