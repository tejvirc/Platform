namespace Aristocrat.Monaco.Sas.UI.Settings
{
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Contracts.SASProperties;
    using Kernel;
    using Localization.Properties;
    using MVVM.Model;
    using Newtonsoft.Json;

    /// <summary>
    ///     Gets the settings for the sas features
    /// </summary>
    public class SasFeaturesSettings : BaseNotify
    {
        private SasHandpayReportingType _handpayReportingType;
        private long _transferLimit;
        private long _maxAllowedTransferLimits;
        private bool _partialTransferAllowed;
        private bool _transferInAllowed;
        private bool _debitTransfersAllowed;
        private bool _transferToTicketAllowed;
        private bool _transferOutAllowed;
        private bool _winTransferAllowed;
        private bool _aftBonusAllowed;
        private bool _legacyBonusAllowed;
        private SasValidationType _validationType;
        private ExceptionOverflowBehavior _overflowBehavior;
        private ConfigNotificationTypes _configNotification;
        private bool _disableOnDisconnect;
        private bool _nonSasProgressiveHitReporting;
        private bool _disabledOnPowerUp;
        private bool _disableOnDisconnectConfigurable;
        private bool _generalControlEditable;
        private bool _addressConfigurableOnlyOnce;
        private bool _bonusTransferStatusEditable;
        private int _progressiveGroupId;

        /// <summary>
        ///     Gets or sets a value indicating the type of handpay reporting supported by the gaming machine
        /// </summary>
        public SasHandpayReportingType HandpayReportingType
        {
            get => _handpayReportingType;
            set => SetProperty(ref _handpayReportingType, value);
        }

        /// <summary>
        ///     Gets or sets the transfer limit
        /// </summary>
        public long TransferLimit
        {
            get => _transferLimit;
            set => SetProperty(ref _transferLimit, value);
        }

        /// <summary>
        ///     Gets the transfer limit display
        /// </summary>
        [JsonIgnore]
        public string AftTransferLimitDisplay
        {
            get
            {
                var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
                var creditLimit = propertiesManager.GetValue(AccountingConstants.MaxCreditMeter, long.MaxValue).MillicentsToDollars();
                return TransferLimit.CentsToDollars() < creditLimit
                    ? TransferLimit.CentsToDollars().FormattedCurrencyString()
                    : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoLimit);
            }
        }

        /// <summary>
        ///     Gets or sets the max allowed transfer limits
        /// </summary>
        public long MaxAllowedTransferLimits
        {
            get => _maxAllowedTransferLimits;
            set => SetProperty(ref _maxAllowedTransferLimits, value);
        }

        /// <summary>
        ///     Gets or sets whether or not partial transfers are allowed
        /// </summary>
        public bool PartialTransferAllowed
        {
            get => _partialTransferAllowed;
            set => SetProperty(ref _partialTransferAllowed, value);
        }

        /// <summary>
        ///     Gets or sets whether or not transfer in is allowed
        /// </summary>
        public bool TransferInAllowed
        {
            get => _transferInAllowed;
            set => SetProperty(ref _transferInAllowed, value);
        }

        /// <summary>
        ///     Gets or sets whether or not debit transfers are allowed
        /// </summary>
        public bool DebitTransfersAllowed
        {
            get => _debitTransfersAllowed;
            set => SetProperty(ref _debitTransfersAllowed, value);
        }

        /// <summary>
        ///     Gets or sets whether or not transfers to tickets are allowed
        /// </summary>
        public bool TransferToTicketAllowed
        {
            get => _transferToTicketAllowed;
            set => SetProperty(ref _transferToTicketAllowed, value);
        }

        /// <summary>
        ///     Gets or sets whether or not transfer out is allowed
        /// </summary>
        public bool TransferOutAllowed
        {
            get => _transferOutAllowed;
            set => SetProperty(ref _transferOutAllowed, value);
        }

        /// <summary>
        ///     Gets or sets whether or not win transfer are allowed
        /// </summary>
        public bool WinTransferAllowed
        {
            get => _winTransferAllowed;
            set => SetProperty(ref _winTransferAllowed, value);
        }

        /// <summary>
        ///     Gets or sets whether or not bonus transfer are allowed
        /// </summary>
        public bool AftBonusAllowed
        {
            get => _aftBonusAllowed;
            set => SetProperty(ref _aftBonusAllowed, value);
        }

        /// <summary>
        ///     Gets or set whether or not legacy bonuses are allowed
        /// </summary>
        public bool LegacyBonusAllowed
        {
            get => _legacyBonusAllowed;
            set => SetProperty(ref _legacyBonusAllowed, value);
        }

        /// <summary>
        ///     Gets or sets the validation type
        /// </summary>
        public SasValidationType ValidationType
        {
            get => _validationType;
            set => SetProperty(ref _validationType, value);
        }

        /// <summary>
        ///     Gets or sets the overflow behavior
        /// </summary>
        public ExceptionOverflowBehavior OverflowBehavior
        {
            get => _overflowBehavior;
            set => SetProperty(ref _overflowBehavior, value);
        }

        /// <summary>
        ///     Gets or sets the configuration change notification
        /// </summary>
        public ConfigNotificationTypes ConfigNotification
        {
            get => _configNotification;
            set => SetProperty(ref _configNotification, value);
        }

        /// <summary>
        ///     Gets or sets whether or not we disable of host disconnect
        /// </summary>
        public bool DisableOnDisconnect
        {
            get => _disableOnDisconnect;
            set => SetProperty(ref _disableOnDisconnect, value);
        }

        /// <summary>
        ///     Gets or sets whether or not Non Sas Progressive Hit Reporting
        /// </summary>
        public bool NonSasProgressiveHitReporting
        {
            get => _nonSasProgressiveHitReporting;
            set => SetProperty(ref _nonSasProgressiveHitReporting, value);
        }

        /// <summary>
        ///     Gets or sets whether or not we disable on power up
        /// </summary>
        public bool DisabledOnPowerUp
        {
            get => _disabledOnPowerUp;
            set => SetProperty(ref _disabledOnPowerUp, value);
        }

        /// <summary>
        ///     Gets or sets whether or not disable on disconnect is configurable
        /// </summary>
        public bool DisableOnDisconnectConfigurable
        {
            get => _disableOnDisconnectConfigurable;
            set => SetProperty(ref _disableOnDisconnectConfigurable, value);
        }

        /// <summary>
        ///     Gets or sets whether or not the general control port is editable
        /// </summary>
        public bool GeneralControlEditable
        {
            get => _generalControlEditable;
            set => SetProperty(ref _generalControlEditable, value);
        }

        /// <summary>
        ///     Gets or sets whether or not the address is configuration only once
        /// </summary>
        public bool AddressConfigurableOnlyOnce
        {
            get => _addressConfigurableOnlyOnce;
            set => SetProperty(ref _addressConfigurableOnlyOnce, value);
        }

        /// <summary>
        ///     Gets or sets whether or not bonus transfers are editable
        /// </summary>
        public bool BonusTransferStatusEditable
        {
            get => _bonusTransferStatusEditable;
            set => SetProperty(ref _bonusTransferStatusEditable, value);
        }

        /// <summary>
        ///     Gets or sets the progressive group id
        /// </summary>
        public int ProgressiveGroupId
        {
            get => _progressiveGroupId;
            set => SetProperty(ref _progressiveGroupId, value);
        }

        /// <summary>
        ///     Performs conversion from <see cref="SasFeaturesSettings"/> to <see cref="SasFeatures"/>.
        /// </summary>
        /// <param name="settings">The <see cref="SasFeaturesSettings"/> settings</param>
        public static explicit operator SasFeatures(SasFeaturesSettings settings) => new SasFeatures
        {
            AddressConfigurableOnlyOnce = settings.AddressConfigurableOnlyOnce,
            ValidationType = settings.ValidationType,
            TransferInAllowed = settings.TransferInAllowed,
            TransferOutAllowed = settings.TransferOutAllowed,
            DisableOnDisconnect = settings.DisableOnDisconnect,
            NonSasProgressiveHitReporting = settings.NonSasProgressiveHitReporting,
            AftBonusAllowed = settings.AftBonusAllowed,
            ConfigNotification = settings.ConfigNotification,
            TransferLimit = settings.TransferLimit,
            MaxAllowedTransferLimits = settings.MaxAllowedTransferLimits,
            PartialTransferAllowed = settings.PartialTransferAllowed,
            BonusTransferStatusEditable = settings.BonusTransferStatusEditable,
            DisableOnDisconnectConfigurable = settings.DisableOnDisconnectConfigurable,
            DisabledOnPowerUp = settings.DisabledOnPowerUp,
            GeneralControlEditable = settings.GeneralControlEditable,
            HandpayReportingType = settings.HandpayReportingType,
            LegacyBonusAllowed = settings.LegacyBonusAllowed,
            OverflowBehavior = settings.OverflowBehavior,
            ProgressiveGroupId = settings.ProgressiveGroupId,
            WinTransferAllowed = settings.WinTransferAllowed,
            DebitTransfersAllowed = settings.DebitTransfersAllowed,
            TransferToTicketAllowed = settings.TransferToTicketAllowed
        };

        /// <summary>
        ///     Performs conversion from <see cref="SasFeatures"/> to <see cref="SasFeaturesSettings"/>.
        /// </summary>
        /// <param name="features">The <see cref="SasFeatures"/> features</param>
        public static explicit operator SasFeaturesSettings(SasFeatures features) => new SasFeaturesSettings
        {
            AddressConfigurableOnlyOnce = features.AddressConfigurableOnlyOnce,
            ValidationType = features.ValidationType,
            TransferInAllowed = features.TransferInAllowed,
            TransferOutAllowed = features.TransferOutAllowed,
            DisableOnDisconnect = features.DisableOnDisconnect,
            NonSasProgressiveHitReporting = features.NonSasProgressiveHitReporting,
            AftBonusAllowed = features.AftBonusAllowed,
            ConfigNotification = features.ConfigNotification,
            TransferLimit = features.TransferLimit,
            MaxAllowedTransferLimits = features.MaxAllowedTransferLimits,
            PartialTransferAllowed = features.PartialTransferAllowed,
            BonusTransferStatusEditable = features.BonusTransferStatusEditable,
            DisableOnDisconnectConfigurable = features.DisableOnDisconnectConfigurable,
            DisabledOnPowerUp = features.DisabledOnPowerUp,
            GeneralControlEditable = features.GeneralControlEditable,
            HandpayReportingType = features.HandpayReportingType,
            LegacyBonusAllowed = features.LegacyBonusAllowed,
            OverflowBehavior = features.OverflowBehavior,
            ProgressiveGroupId = features.ProgressiveGroupId,
            WinTransferAllowed = features.WinTransferAllowed,
            DebitTransfersAllowed = features.DebitTransfersAllowed,
            TransferToTicketAllowed = features.TransferToTicketAllowed
        };
    }
}