namespace Aristocrat.Monaco.G2S.Services
{
    using Accounting.Contracts.Handpay;

    public interface IHandpayProperties
    {
        /// <summary>
        ///     Gets or sets a device identifier of the idReader device associated with the handpay device; set to
        ///     0 (zero) if there is no associated idReader
        ///device.
        /// </summary>
        int IdReaderId { get; set; }

        /// <summary>
        ///     Get or sets a value indicating whether the EGM MUST use the idReader device associated with the currently active player session; otherwise,
        ///     the EGM MUST use the idReader device specified in the idReaderId attribute of the handpayProfile command.
        /// </summary>
        bool UsePlayerIdReader { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether non-validated receipts are enabled.
        /// </summary>
        bool EnableReceipts { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether local handpay is enabled when the device is enabled
        /// </summary>
        bool EnabledLocalHandpay { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether local credit handpay is enabled when the device is enabled
        /// </summary>
        bool EnabledLocalCredit { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether local voucher handpay is enabled when the device is enabled
        /// </summary>
        bool EnabledLocalVoucher { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether local WAT handpay is enabled when the device is enabled
        /// </summary>
        bool EnabledLocalWat { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether remote handpay is enabled when the device is enabled
        /// </summary>
        bool EnabledRemoteHandpay { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether remote credit handpay is enabled when the device is enabled
        /// </summary>
        bool EnabledRemoteCredit { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether remote voucher handpay is enabled when the device is enabled
        /// </summary>
        bool EnabledRemoteVoucher { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether remote WAT handpay is enabled when the device is enabled
        /// </summary>
        bool EnabledRemoteWat { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether local handpay is enabled when the device is disabled
        /// </summary>
        bool DisabledLocalHandpay { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether local credit handpay is enabled when the device is disabled
        /// </summary>
        bool DisabledLocalCredit { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether local voucher handpay is enabled when the device is disabled
        /// </summary>
        bool DisabledLocalVoucher { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether local WAT handpay is enabled when the device is disabled
        /// </summary>
        bool DisabledLocalWat { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether remote handpay is enabled when the device is disabled
        /// </summary>
        bool DisabledRemoteHandpay { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether remote credit handpay is enabled when the device is disabled
        /// </summary>
        bool DisabledRemoteCredit { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether remote voucher handpay is enabled when the device is disabled
        /// </summary>
        bool DisabledRemoteVoucher { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether remote WAT handpay is enabled when the device is disabled
        /// </summary>
        bool DisabledRemoteWat { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether local handpay is enabled
        /// </summary>
        LocalKeyOff LocalKeyOff { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether local handpay is enabled
        /// </summary>
        bool PartialHandpays { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the EGM is allowed to
        ///     include multiple credit types in a single handpay request
        /// </summary>
        bool MixCreditTypes { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the EGM may include
        ///     non-cashable credits in handpay requests
        /// </summary>
        bool RequestNonCash { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether promotional credits
        ///     MUST be converted to cashable credits when paid as a handpay
        /// </summary>
        bool CombineCashableOut { get; set; }

        /// <summary>
        ///     Gets a value indicating whether local handpay is allowed
        /// </summary>
        bool AllowLocalHandpay { get; }

        /// <summary>
        ///     Gets a value indicating whether local handpay is allowed
        /// </summary>
        bool AllowLocalVoucher { get; }

        /// <summary>
        ///     Gets a value indicating whether local handpay is allowed
        /// </summary>
        bool AllowLocalCredit { get; }

        /// <summary>
        ///     Gets a value indicating whether local handpay is allowed
        /// </summary>
        bool AllowLocalWat { get; }

        /// <summary>
        ///     Gets a value indicating whether local handpay is allowed
        /// </summary>
        bool AllowRemoteHandpay { get; }

        /// <summary>
        ///     Gets a value indicating whether local handpay is allowed
        /// </summary>
        bool AllowRemoteVoucher { get; }

        /// <summary>
        ///     Gets a value indicating whether local handpay is allowed
        /// </summary>
        bool AllowRemoteCredit { get; }

        /// <summary>
        ///     Gets a value indicating whether local handpay is allowed
        /// </summary>
        bool AllowRemoteWat { get; }

        /// <summary>
        ///     Gets the title printed on game win and external bonus receipts
        /// </summary>
        string TitleJackpotReceipt { get; set; }

        /// <summary>
        ///     Gets the title printed on cancel credit receipts
        /// </summary>
        string TitleCancelReceipt { get; set; }
    }
}
