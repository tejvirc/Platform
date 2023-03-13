namespace Aristocrat.Monaco.Accounting.UI.Settings
{
    using System;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Contracts;
    using Contracts.Handpay;
    using Localization.Properties;
    using MVVM.Model;
    using Newtonsoft.Json;

    /// <summary>
    ///     Accounting machine settings.
    /// </summary>
    internal class MachineSettings : BaseNotify
    {
        private bool _allowCashWinTicket;
        private bool _allowCreditUnderLimit;
        private long _celebrationLockupLimit;
        private CheckCreditsStrategy _checkCreditsIn;
        private bool _combineCashableOut;
        private bool _disabledLocalCredit;
        private bool _disabledLocalHandpay;
        private bool _disabledLocalVoucher;
        private bool _disabledLocalWat;
        private bool _disabledRemoteCredit;
        private bool _disabledRemoteHandpay;
        private bool _disabledRemoteVoucher;
        private bool _disabledRemoteWat;
        private bool _enabledLocalCredit;
        private bool _enabledLocalHandpay;
        private bool _enabledLocalVoucher;
        private bool _enabledLocalWat;
        private bool _enabledRemoteCredit;
        private bool _enabledRemoteHandpay;
        private bool _enabledRemoteVoucher;
        private bool _enabledRemoteWat;
        private bool _enableReceipts;
        private int _idReaderId;
        private bool _ignoreVoucherStackedDuringReboot;
        private LocalKeyOff _localKeyOff;
        private long _largeWinLimit;
        private bool _overwriteLargeWinLimit;
        private long _largeWinRatio;
        private bool _overwriteLargeWinRatio;
        private bool _largeWinRatioIsChecked;
        private long _largeWinRatioThreshold;
        private bool _overwriteLargeWinRatioThreshold;
        private bool _largeWinRatioThresholdIsChecked;
        private long _maxBetLimit;
        private bool _overwriteMaxBetLimit;
        private long _maxCreditMeter;
        private long _maxCreditMeterMaxAllowed;
        private long _maxTenderInLimit;
        private long _maxWinAmount;
        private bool _mixCreditTypes;
        private bool _moneyInEnabled;
        private bool _partialHandpays;
        private string _redeemText;
        private string _reprintLoggedVoucherBehavior;
        private string _reprintLoggedVoucherDoorOpenRequirement;
        private bool _reprintLoggedVoucherTitleOverride;
        private bool _requestNonCash;
        private int _ticketBarcodeLength;
        private string _ticketTitleBonusCash;
        private string _ticketTitleBonusNonCash;
        private string _ticketTitleBonusPromo;
        private string _ticketTitleCash;
        private string _ticketTitleLargeWin;
        private string _ticketTitleNonCash;
        private string _ticketTitlePromo;
        private string _ticketTitleWatNonCash;
        private string _ticketTitleWatPromo;
        private string _titleCancelReceipt;
        private string _titleJackpotReceipt;
        private bool _usePlayerIdReader;
        private bool _validateHandpays;
        private long _voucherInLimit;
        private bool _voucherOut;
        private bool _voucherOutCheckBoxChecked;
        private bool _voucherInCheckBoxChecked;
        private int _voucherOutExpirationDays;
        private long _voucherOutLimit;
        private bool _voucherOutNonCash;
        private int _voucherOutNonCashExpirationDays;
        private long _handpayLimit;
        private bool _allowRemoteHandpayReset;
        private LargeWinHandpayResetMethod _largeWinHandpayResetMethod;
        private bool _handpayLimitIsChecked;
        private bool _largeWinLimitIsChecked;
        private bool _creditLimitIsChecked;
        private bool _maxBetLimitIsChecked;

        /// <summary>
        ///     Gets or sets a value that indicates whether to allow cash win ticket.
        /// </summary>
        public bool AllowCashWinTicket
        {
            get => _allowCashWinTicket;

            set => SetProperty(ref _allowCashWinTicket, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether to allow credit under limit.
        /// </summary>
        public bool AllowCreditUnderLimit
        {
            get => _allowCreditUnderLimit;

            set => SetProperty(ref _allowCreditUnderLimit, value);
        }

        /// <summary>
        ///     Gets or sets the celebration lockup limit.
        /// </summary>
        public long CelebrationLockupLimit
        {
            get => _celebrationLockupLimit;

            set
            {
                SetProperty(ref _celebrationLockupLimit, value);
                RaisePropertyChanged(nameof(CelebrationLockupLimitDisplay));
            }
        }

        /// <summary>
        ///     Gets the celebration lockup limit to display.
        /// </summary>
        [JsonIgnore]
        public string CelebrationLockupLimitDisplay =>
            _celebrationLockupLimit > 0
                ? _celebrationLockupLimit.MillicentsToDollars().FormattedCurrencyString()
                : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoLimit);

        /// <summary>
        ///     Gets or sets the check credits in strategy.
        /// </summary>
        public CheckCreditsStrategy CheckCreditsIn
        {
            get => _checkCreditsIn;

            set => SetProperty(ref _checkCreditsIn, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether to combine cashable out.
        /// </summary>
        public bool CombineCashableOut
        {
            get => _combineCashableOut;

            set => SetProperty(ref _combineCashableOut, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether local credit handpay is disabled.
        /// </summary>
        public bool DisabledLocalCredit
        {
            get => _disabledLocalCredit;

            set => SetProperty(ref _disabledLocalCredit, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether local handpay is disabled.
        /// </summary>
        public bool DisabledLocalHandpay
        {
            get => _disabledLocalHandpay;

            set => SetProperty(ref _disabledLocalHandpay, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether local voucher handpay is disabled.
        /// </summary>
        public bool DisabledLocalVoucher
        {
            get => _disabledLocalVoucher;

            set => SetProperty(ref _disabledLocalVoucher, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether local WAT handpay is disabled.
        /// </summary>
        public bool DisabledLocalWat
        {
            get => _disabledLocalWat;

            set => SetProperty(ref _disabledLocalWat, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether remote credit handpay is disabled.
        /// </summary>
        public bool DisabledRemoteCredit
        {
            get => _disabledRemoteCredit;

            set => SetProperty(ref _disabledRemoteCredit, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether remote handpay is disabled.
        /// </summary>
        public bool DisabledRemoteHandpay
        {
            get => _disabledRemoteHandpay;

            set => SetProperty(ref _disabledRemoteHandpay, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether remote voucher handpay is disabled.
        /// </summary>
        public bool DisabledRemoteVoucher
        {
            get => _disabledRemoteVoucher;

            set => SetProperty(ref _disabledRemoteVoucher, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether remote WAT handpay is disabled.
        /// </summary>
        public bool DisabledRemoteWat
        {
            get => _disabledRemoteWat;

            set => SetProperty(ref _disabledRemoteWat, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether local credit handpay is enabled.
        /// </summary>
        public bool EnabledLocalCredit
        {
            get => _enabledLocalCredit;

            set => SetProperty(ref _enabledLocalCredit, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether local handpay is enabled.
        /// </summary>
        public bool EnabledLocalHandpay
        {
            get => _enabledLocalHandpay;

            set => SetProperty(ref _enabledLocalHandpay, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether local voucher handpay is enabled.
        /// </summary>
        public bool EnabledLocalVoucher
        {
            get => _enabledLocalVoucher;

            set => SetProperty(ref _enabledLocalVoucher, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether local WAT handpay is enabled.
        /// </summary>
        public bool EnabledLocalWat
        {
            get => _enabledLocalWat;

            set => SetProperty(ref _enabledLocalWat, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether remote credit handpay is enabled.
        /// </summary>
        public bool EnabledRemoteCredit
        {
            get => _enabledRemoteCredit;

            set => SetProperty(ref _enabledRemoteCredit, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether remote handpay is enabled.
        /// </summary>
        public bool EnabledRemoteHandpay
        {
            get => _enabledRemoteHandpay;

            set => SetProperty(ref _enabledRemoteHandpay, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether remote voucher handpay is enabled.
        /// </summary>
        public bool EnabledRemoteVoucher
        {
            get => _enabledRemoteVoucher;

            set => SetProperty(ref _enabledRemoteVoucher, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether remote WAT handpay is enabled.
        /// </summary>
        public bool EnabledRemoteWat
        {
            get => _enabledRemoteWat;

            set => SetProperty(ref _enabledRemoteWat, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether non-validated receipts is enabled.
        /// </summary>
        public bool EnableReceipts
        {
            get => _enableReceipts;

            set => SetProperty(ref _enableReceipts, value);
        }

        /// <summary>
        ///     Gets or sets the ID Reader ID.
        /// </summary>
        public int IdReaderId
        {
            get => _idReaderId;

            set => SetProperty(ref _idReaderId, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether to ignore voucher stacker during reboot.
        /// </summary>
        public bool IgnoreVoucherStackedDuringReboot
        {
            get => _ignoreVoucherStackedDuringReboot;

            set => SetProperty(ref _ignoreVoucherStackedDuringReboot, value);
        }

        /// <summary>
        ///     Gets or sets the allowed local handpay key-off.
        /// </summary>
        public LocalKeyOff LocalKeyOff
        {
            get => _localKeyOff;

            set => SetProperty(ref _localKeyOff, value);
        }

        /// <summary>
        ///     Gets or sets the large win limit.
        /// </summary>
        public long LargeWinLimit
        {
            get => _largeWinLimit;

            set
            {
                SetProperty(ref _largeWinLimit, value);
                RaisePropertyChanged(nameof(LargeWinLimitDisplay));
            }
        }

        /// <summary>
        ///     Gets the large win limit to display.
        /// </summary>
        [JsonIgnore]
        public string LargeWinLimitDisplay =>
            _largeWinLimit < AccountingConstants.DefaultLargeWinLimit
                ? _largeWinLimit.MillicentsToDollars().FormattedCurrencyString()
                : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoLimit);

        /// <summary>
        ///     Gets or sets the overwrite large win limit.
        /// </summary>
        public bool OverwriteLargeWinLimit
        {
            get => _overwriteLargeWinLimit;

            set => SetProperty(ref _overwriteLargeWinLimit, value);
        }

        /// <summary>
        ///     Gets or sets the large win ratio.
        /// </summary>
        public long LargeWinRatio
        {
            get => _largeWinRatio;

            set
            {
                SetProperty(ref _largeWinRatio, value);
                RaisePropertyChanged(nameof(LargeWinRatioDisplay));
            }
        }

        /// <summary>
        ///     Gets the large win ratio to display.
        /// </summary>
        [JsonIgnore]
        public string LargeWinRatioDisplay =>
            _largeWinRatio > AccountingConstants.DefaultLargeWinRatio
                ? string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.RatioDisplayFormatter), _largeWinRatio / 100.0m)
                : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoLimit);

        /// <summary>
        ///     Gets or sets the overwrite large win ratio.
        /// </summary>
        public bool OverwriteLargeWinRatio
        {
            get => _overwriteLargeWinRatio;

            set => SetProperty(ref _overwriteLargeWinRatio, value);
        }

        /// <summary>
        ///     Gets or sets the large win limit is checked value.
        /// </summary>
        public bool LargeWinRatioIsChecked
        {
            get => _largeWinRatioIsChecked;

            set => SetProperty(ref _largeWinRatioIsChecked, value);
        }

        /// <summary>
        ///     Gets or sets the large win ratio.
        /// </summary>
        public long LargeWinRatioThreshold
        {
            get => _largeWinRatioThreshold;

            set
            {
                SetProperty(ref _largeWinRatioThreshold, value);
                RaisePropertyChanged(nameof(LargeWinRatioThresholdDisplay));
            }
        }

        /// <summary>
        ///     Gets the large win ratio to display.
        /// </summary>
        [JsonIgnore]
        public string LargeWinRatioThresholdDisplay =>
            _largeWinRatioThreshold > AccountingConstants.DefaultLargeWinRatioThreshold
                ? _largeWinRatioThreshold.MillicentsToDollars().FormattedCurrencyString()
                : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoLimit);

        /// <summary>
        ///     Gets or sets the overwrite large win ratio.
        /// </summary>
        public bool OverwriteLargeWinRatioThreshold
        {
            get => _overwriteLargeWinRatioThreshold;

            set => SetProperty(ref _overwriteLargeWinRatioThreshold, value);
        }

        /// <summary>
        ///     Gets or sets the large win limit is checked value.
        /// </summary>
        public bool LargeWinRatioThresholdIsChecked
        {
            get => _largeWinRatioThresholdIsChecked;

            set => SetProperty(ref _largeWinRatioThresholdIsChecked, value);
        }

        /// <summary>
        ///     Gets or sets max bet limit.
        /// </summary>
        public long MaxBetLimit
        {
            get => _maxBetLimit;

            set
            {
                SetProperty(ref _maxBetLimit, value);
                RaisePropertyChanged(nameof(MaxBetLimitDisplay));
            }
        }

        /// <summary>
        ///     Gets the max bet limit to display.
        /// </summary>
        [JsonIgnore]
        public string MaxBetLimitDisplay =>
            _maxBetLimit < long.MaxValue
                ? _maxBetLimit.MillicentsToDollars().FormattedCurrencyString()
                : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoLimit);

        /// <summary>
        ///     Gets or sets overwrite max bet limit.
        /// </summary>
        public bool OverwriteMaxBetLimit
        {
            get => _overwriteMaxBetLimit;

            set => SetProperty(ref _overwriteMaxBetLimit, value);
        }

        /// <summary>
        ///     Gets or sets the max credit meter.
        /// </summary>
        public long MaxCreditMeter
        {
            get => _maxCreditMeter;

            set
            {
                SetProperty(ref _maxCreditMeter, value);
                RaisePropertyChanged(nameof(MaxCreditMeterDisplay));
            }
        }

        /// <summary>
        ///     Gets the max credit meter to display.
        /// </summary>
        [JsonIgnore]
        public string MaxCreditMeterDisplay =>
            _maxCreditMeter <= _maxCreditMeterMaxAllowed
                ? _maxCreditMeter.MillicentsToDollars().FormattedCurrencyString()
                : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoLimit);

        /// <summary>
        ///     Gets or sets the max credit meter max allowed.
        /// </summary>
        public long MaxCreditMeterMaxAllowed
        {
            get => _maxCreditMeterMaxAllowed;

            set => SetProperty(ref _maxCreditMeterMaxAllowed, value);
        }

        /// <summary>
        ///     Gets or sets the max tender in limit.
        /// </summary>
        public long MaxTenderInLimit
        {
            get => _maxTenderInLimit;

            set
            {
                SetProperty(ref _maxTenderInLimit, value);
                RaisePropertyChanged(nameof(MaxTenderInLimitDisplay));
            }
        }

        /// <summary>
        ///     Gets the max tender in limit to display.
        /// </summary>
        [JsonIgnore]
        public string MaxTenderInLimitDisplay => _maxTenderInLimit.MillicentsToDollars().FormattedCurrencyString();
                 
        /// <summary>
        ///     Gets or sets the max win amount.
        /// </summary>
        public long MaxWinAmount
        {
            get => _maxWinAmount;

            set
            {
                SetProperty(ref _maxWinAmount, value);
                RaisePropertyChanged(nameof(MaxWinAmountDisplay));
            }
        }

        /// <summary>
        ///     Gets the max win amount to display.
        /// </summary>
        [JsonIgnore]
        public string MaxWinAmountDisplay =>
            _maxWinAmount > 0
                ? _maxWinAmount.MillicentsToDollars().FormattedCurrencyString()
                : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoLimit);

        /// <summary>
        ///     Gets or sets a value that indicates whether to allow mix credit types.
        /// </summary>
        public bool MixCreditTypes
        {
            get => _mixCreditTypes;

            set => SetProperty(ref _mixCreditTypes, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether to accept cash.
        /// </summary>
        public bool MoneyInEnabled
        {
            get => _moneyInEnabled;

            set => SetProperty(ref _moneyInEnabled, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether to allow partial handpays.
        /// </summary>
        public bool PartialHandpays
        {
            get => _partialHandpays;

            set => SetProperty(ref _partialHandpays, value);
        }

        /// <summary>
        ///     Gets or sets the redeem text.
        /// </summary>
        public string RedeemText
        {
            get => _redeemText;

            set => SetProperty(ref _redeemText, value);
        }

        /// <summary>
        ///     Gets or sets the reprint logged voucher behavior.
        /// </summary>
        public string ReprintLoggedVoucherBehavior
        {
            get => _reprintLoggedVoucherBehavior;

            set => SetProperty(ref _reprintLoggedVoucherBehavior, value);
        }

        /// <summary>
        ///     Gets or sets the reprint logged voucher door open requirement.
        /// </summary>
        public string ReprintLoggedVoucherDoorOpenRequirement
        {
            get => _reprintLoggedVoucherDoorOpenRequirement;

            set => SetProperty(ref _reprintLoggedVoucherDoorOpenRequirement, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether to enable reprint logged voucher title override.
        /// </summary>
        public bool ReprintLoggedVoucherTitleOverride
        {
            get => _reprintLoggedVoucherTitleOverride;

            set => SetProperty(ref _reprintLoggedVoucherTitleOverride, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether to allow non-cash request.
        /// </summary>
        public bool RequestNonCash
        {
            get => _requestNonCash;

            set => SetProperty(ref _requestNonCash, value);
        }

        /// <summary>
        ///     Gets or sets the ticket barcode length.
        /// </summary>
        public int TicketBarcodeLength
        {
            get => _ticketBarcodeLength;

            set => SetProperty(ref _ticketBarcodeLength, value);
        }

        /// <summary>
        ///     Gets or sets the ticket title bonus cash.
        /// </summary>
        public string TicketTitleBonusCash
        {
            get => _ticketTitleBonusCash;

            set => SetProperty(ref _ticketTitleBonusCash, value);
        }

        /// <summary>
        ///     Gets or sets the ticket title bonus non-cash.
        /// </summary>
        public string TicketTitleBonusNonCash
        {
            get => _ticketTitleBonusNonCash;

            set => SetProperty(ref _ticketTitleBonusNonCash, value);
        }

        /// <summary>
        ///     Gets or sets the ticket title bonus promo.
        /// </summary>
        public string TicketTitleBonusPromo
        {
            get => _ticketTitleBonusPromo;

            set => SetProperty(ref _ticketTitleBonusPromo, value);
        }

        /// <summary>
        ///     Gets or sets the ticket title cash.
        /// </summary>
        public string TicketTitleCash
        {
            get => _ticketTitleCash;

            set => SetProperty(ref _ticketTitleCash, value);
        }

        /// <summary>
        ///     Gets or sets the ticket title large win.
        /// </summary>
        public string TicketTitleLargeWin
        {
            get => _ticketTitleLargeWin;

            set => SetProperty(ref _ticketTitleLargeWin, value);
        }

        /// <summary>
        ///     Gets or sets the ticket title non-cash.
        /// </summary>
        public string TicketTitleNonCash
        {
            get => _ticketTitleNonCash;

            set
            {
                SetProperty(ref _ticketTitleNonCash, value);
                RaisePropertyChanged(nameof(TicketTitleNonCashDisplay));
            }
        }

        /// <summary>
        ///     Gets the ticket title non-cash to display.
        /// </summary>
        [JsonIgnore]
        public string TicketTitleNonCashDisplay =>
            string.IsNullOrEmpty(_ticketTitleNonCash)
                ? Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.PlayableOnly)
                : _ticketTitleNonCash;

        /// <summary>
        ///     Gets or sets the ticket title promo.
        /// </summary>
        public string TicketTitlePromo
        {
            get => _ticketTitlePromo;

            set => SetProperty(ref _ticketTitlePromo, value);
        }

        /// <summary>
        ///     Gets or sets the ticket title WAT non-cash.
        /// </summary>
        public string TicketTitleWatNonCash
        {
            get => _ticketTitleWatNonCash;

            set => SetProperty(ref _ticketTitleWatNonCash, value);
        }

        /// <summary>
        ///     Gets or sets the ticket title WAT promo.
        /// </summary>
        public string TicketTitleWatPromo
        {
            get => _ticketTitleWatPromo;

            set => SetProperty(ref _ticketTitleWatPromo, value);
        }

        /// <summary>
        ///     Gets or sets the ticket title cancel receipts.
        /// </summary>
        public string TitleCancelReceipt
        {
            get => _titleCancelReceipt;

            set
            {
                SetProperty(ref _titleCancelReceipt, value);
                RaisePropertyChanged(nameof(TitleCancelReceiptDisplay));
            }
        }

        /// <summary>
        ///     Gets the title cancel receipt to display.
        /// </summary>
        [JsonIgnore]
        public string TitleCancelReceiptDisplay =>
            string.IsNullOrEmpty(_titleCancelReceipt)
                ? Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.HandpayReceipt)
                : _titleCancelReceipt;

        /// <summary>
        ///     Gets or sets the ticket title jackpot receipt.
        /// </summary>
        public string TitleJackpotReceipt
        {
            get => _titleJackpotReceipt;

            set
            {
                SetProperty(ref _titleJackpotReceipt, value);
                RaisePropertyChanged(nameof(TitleJackpotReceiptDisplay));
            }
        }

        /// <summary>
        ///     Gets the title jackpot receipt for display
        /// </summary>
        public string TitleJackpotReceiptDisplay =>
            string.IsNullOrEmpty(_titleJackpotReceipt)
                ? Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.JackpotHandpayTicket)
                : _titleJackpotReceipt;

        /// <summary>
        ///     Gets or sets a value that indicates whether to use player ID Reader.
        /// </summary>
        public bool UsePlayerIdReader
        {
            get => _usePlayerIdReader;

            set => SetProperty(ref _usePlayerIdReader, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether to validate handpays.
        /// </summary>
        public bool ValidateHandpays
        {
            get => _validateHandpays;

            set => SetProperty(ref _validateHandpays, value);
        }

        /// <summary>
        ///     Gets or sets the voucher in limit.
        /// </summary>
        public long VoucherInLimit
        {
            get => _voucherInLimit;

            set
            {
                SetProperty(ref _voucherInLimit, value);
                RaisePropertyChanged(nameof(VoucherInLimitDisplay));
            }
        }

        /// <summary>
        ///     Gets the voucher in limit to display.
        /// </summary>
        [JsonIgnore]
        public string VoucherInLimitDisplay =>
            _voucherInLimit < Math.Min(_maxCreditMeterMaxAllowed, long.MaxValue)
                ? _voucherInLimit.MillicentsToDollars().FormattedCurrencyString()
                : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoLimit);

        /// <summary>
        ///     Gets or sets a value that indicates whether to allow voucher out.
        /// </summary>
        public bool VoucherOut
        {
            get => _voucherOut;

            set => SetProperty(ref _voucherOut, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether Voucher in checkbox  is checked or not..
        /// </summary>
        public bool VoucherInCheckBoxChecked
        {
            get => _voucherInCheckBoxChecked;

            set => SetProperty(ref _voucherInCheckBoxChecked, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether Voucher out checkbox  is checked or not.
        /// </summary>
        public bool VoucherOutCheckBoxChecked
        {
            get => _voucherOutCheckBoxChecked;

            set => SetProperty(ref _voucherOutCheckBoxChecked, value);
        }

        /// <summary>
        ///     Gets or sets the voucher out expiration.
        /// </summary>
        public int VoucherOutExpirationDays
        {
            get => _voucherOutExpirationDays;

            set
            {
                SetProperty(ref _voucherOutExpirationDays, value);
                RaisePropertyChanged(nameof(VoucherOutExpirationDaysDisplay));
            }
        }

        /// <summary>
        ///     Gets the voucher out expiration days to display.
        /// </summary>
        [JsonIgnore]
        public string VoucherOutExpirationDaysDisplay =>
            _voucherOutExpirationDays == 0
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NeverExpires)
                : string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DaysFormatter), _voucherOutExpirationDays);

        /// <summary>
        ///     Gets or sets the voucher out limit.
        /// </summary>
        public long VoucherOutLimit
        {
            get => _voucherOutLimit;

            set
            {
                SetProperty(ref _voucherOutLimit, value);
                RaisePropertyChanged(nameof(VoucherOutLimitDisplay));
            }
        }

        /// <summary>
        ///     Gets the voucher out limit to display.
        /// </summary>
        [JsonIgnore]
        public string VoucherOutLimitDisplay =>
            _voucherOutLimit < long.MaxValue
                ? _voucherOutLimit.MillicentsToDollars().FormattedCurrencyString()
                : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoLimit);

        /// <summary>
        ///     Gets or sets a value that indicates whether to allow voucher out non-cash.
        /// </summary>
        public bool VoucherOutNonCash
        {
            get => _voucherOutNonCash;

            set => SetProperty(ref _voucherOutNonCash, value);
        }

        /// <summary>
        ///     Gets or sets the voucher out non-cash expiration days.
        /// </summary>
        public int VoucherOutNonCashExpirationDays
        {
            get => _voucherOutNonCashExpirationDays;

            set 
            {
                SetProperty(ref _voucherOutNonCashExpirationDays, value);
                RaisePropertyChanged(nameof(VoucherOutNonCashExpirationDaysDisplay));
            }
        }

        /// <summary>
        ///     Gets the voucher out non-cash expiration days to display.
        /// </summary>
        [JsonIgnore]
        public string VoucherOutNonCashExpirationDaysDisplay =>
            _voucherOutNonCashExpirationDays == 0
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NeverExpires)
                : string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DaysFormatter), _voucherOutNonCashExpirationDays);

        /// <summary>
        ///     Gets or sets the handpay limit.
        /// </summary>
        public long HandpayLimit
        {
            get => _handpayLimit;

            set
            {
                SetProperty(ref _handpayLimit, value);
                RaisePropertyChanged(nameof(HandpayLimitDisplay));
            }
        }

        /// <summary>
        ///     Gets the handpay limit to display.
        /// </summary>
        [JsonIgnore]
        public string HandpayLimitDisplay =>
            _handpayLimit < AccountingConstants.DefaultHandpayLimit
                ? _handpayLimit.MillicentsToDollars().FormattedCurrencyString()
                : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoLimit);

        /// <summary>
        ///     Gets or sets a value that indicates whether to allow remote handpay reset.
        /// </summary>
        public bool AllowRemoteHandpayReset
        {
            get => _allowRemoteHandpayReset;

            set => SetProperty(ref _allowRemoteHandpayReset, value);
        }

        /// <summary>
        ///     Gets or sets the large win handpay reset method.
        /// </summary>
        public LargeWinHandpayResetMethod LargeWinHandpayResetMethod
        {
            get => _largeWinHandpayResetMethod;

            set => SetProperty(ref _largeWinHandpayResetMethod, value);
        }

        /// <summary>
        ///     Gets or sets the handpay limit is checked value.
        /// </summary>
        public bool HandpayLimitIsChecked
        {
            get => _handpayLimitIsChecked;

            set => SetProperty(ref _handpayLimitIsChecked, value);
        }

        /// <summary>
        ///     Gets or sets the large win limit is checked value.
        /// </summary>
        public bool LargeWinLimitIsChecked
        {
            get => _largeWinLimitIsChecked;

            set => SetProperty(ref _largeWinLimitIsChecked, value);
        }

        /// <summary>
        ///     Gets or sets the credit limit is checked value.
        /// </summary>
        public bool CreditLimitIsChecked
        {
            get => _creditLimitIsChecked;

            set => SetProperty(ref _creditLimitIsChecked, value);
        }

        /// <summary>
        ///     Gets or sets the max bet limit is checked value.
        /// </summary>
        public bool MaxBetLimitIsChecked
        {
            get => _maxBetLimitIsChecked;

            set => SetProperty(ref _maxBetLimitIsChecked, value);
        }
    }
}
