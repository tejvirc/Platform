namespace Aristocrat.Monaco.Sas.Contracts.Metering
{
    /// <summary>
    ///     Class that defines constants for Monaco meter names of Sas specific meters.
    /// </summary>
    public static class SasMeterNames
    {
        /// <summary>
        /// canceled credits amount
        /// </summary>
        public const string TotalCanceledCredits = "Sas.TotalCanceledCredits";

        /// <summary>
        /// Validated canceled credit handpay, receipt printed (quantity) (0x008D)
        /// </summary>
        public const string ValidatedCanceledCreditHandPayReceiptCount =
                            "Sas.ValidatedCanceledCreditHandPayReceiptCount";

        /// <summary>
        /// The meter name for Validated jackpot handpay, receipt printed (cents) (0x008E)
        /// </summary>
        public const string ValidatedJackpotHandPayReceiptCents =
                            "Sas.ValidatedJackpotHandPayReceiptCents";

        /// <summary>
        /// The meter name for Validated jackpot handpay, receipt printed (quantity) (0x008F)
        /// </summary>
        public const string ValidatedJackpotHandPayReceiptCount =
                            "Sas.ValidatedJackpotHandPayReceiptCount";

        /// <summary>
        /// The meter name for Validated canceled credit handpay, no receipt printed (cents) (0x0090)
        /// </summary>
        public const string ValidatedCanceledCreditHandPayNoReceiptCents =
                            "Sas.ValidatedCanceledCreditHandPayNoReceiptCents";

        /// <summary>
        /// The meter name for Validated canceled credit handpay, no receipt printed (quantity) (0x0091)
        /// </summary>
        public const string ValidatedCanceledCreditHandPayNoReceiptCount =
                            "Sas.ValidatedCanceledCreditHandPayNoReceiptCount";

        /// <summary>
        /// The meter name for Validated jackpot handpay, no receipt printed (cents) (0x0092)
        /// </summary>
        public const string ValidatedJackpotHandPayNoReceiptCents =
                            "Sas.ValidatedJackpotHandPayNoReceiptCents";

        /// <summary>
        /// The meter name for Validated jackpot handpay, no receipt printed (quantity) (0x0093)
        /// </summary>
        public const string ValidatedJackpotHandPayNoReceiptCount =
                            "Sas.ValidatedJackpotHandPayNoReceiptCount";

        /// <summary>
        /// The meter name for in-house cashable transfers to gaming machine (0x00A0)
        /// </summary>
        public const string AftCashableIn =
                            "Sas.AftTransfer.AftCashableIn";

        /// <summary>
        /// The meter name for in-house cashable transfers to gaming machine ( quantity ) (0x00A1)
        /// </summary>
        public const string AftCashableInQuantity =
                            "Sas.AftTransfer.AftCashableInQuantity";

        /// <summary>
        /// The meter name for in-house restricted transfers to gaming machine (0x00A2)
        /// </summary>
        public const string AftRestrictedIn =
                            "Sas.AftTransfer.AftRestrictedIn";

        /// <summary>
        /// The meter name for in-house restricted transfers to gaming machine ( quantity ) (0x00A3)
        /// </summary>
        public const string AftRestrictedInQuantity =
                            "Sas.AftTransfer.AftRestrictedInQuantity";

        /// <summary>
        /// The meter name for in-house non-restricted transfers to gaming machine (0x00A4)
        /// </summary>
        public const string AftNonRestrictedIn =
                            "Sas.AftTransfer.AftNonRestrictedIn";

        /// <summary>
        /// The meter name for in-house non-restricted transfers to gaming machine ( quantity ) (0x00A5)
        /// </summary>
        public const string AftNonRestrictedInQuantity =
                            "Sas.AftTransfer.AftNonRestrictedInQuantity";

        /// <summary>
        /// The meter name for bonus cashable transfers to gaming machine (0x00AE)
        /// </summary>
        public const string AftCashableBonusIn =
                            "Sas.AftTransfer.AftCashableBonusIn";

        /// <summary>
        /// The meter name for bonus cashable transfers to gaming machine ( quantity ) (0x00AF)
        /// </summary>
        public const string AftCashableBonusInQuantity =
                            "Sas.AftTransfer.AftCashableBonusInQuantity";

        /// <summary>
        /// The meter name for bonus non-restricted transfers to gaming machine (0x00B0)
        /// </summary>
        public const string AftNonRestrictedBonusIn =
                            "Sas.AftTransfer.AftNonRestrictedBonusIn";

        /// <summary>
        /// The meter name for bonus non-restricted transfers to gaming machine ( quantity ) (0x00B1)
        /// </summary>
        public const string AftNonRestrictedBonusInQuantity =
                            "Sas.AftTransfer.AftNonRestrictedBonusInQuantity";

        /// <summary>
        /// The meter name for in-house cashable transfers to host (0x00B8)
        /// </summary>
        public const string AftCashableOut =
                            "Sas.AftTransfer.AftCashableOut";

        /// <summary>
        /// The meter name for in-house cashable transfers to host ( quantity ) (0x00B9)
        /// </summary>
        public const string AftCashableOutQuantity =
                            "Sas.AftTransfer.AftCashableOutQuantity";

        /// <summary>
        /// The meter name for in-house restricted transfers to host (0x00BA)
        /// </summary>
        public const string AftRestrictedOut =
                            "Sas.AftTransfer.AftRestrictedOut";

        /// <summary>
        /// The meter name for in-house restricted transfers to host ( quantity ) (0x00BB)
        /// </summary>
        public const string AftRestrictedOutQuantity =
                            "Sas.AftTransfer.AftRestrictedOutQuantity";

        /// <summary>
        /// The meter name for in-house non-restricted transfers to host (0x00BC)
        /// </summary>
        public const string AftNonRestrictedOut =
                            "Sas.AftTransfer.AftNonRestrictedOut";

        /// <summary>
        /// The meter name for in-house non-restricted transfers to host ( quantity ) (0x00BD)
        /// </summary>
        public const string AftNonRestrictedOutQuantity =
                            "Sas.AftTransfer.AftNonRestrictedOutQuantity";

        /// <summary>
        ///     Aft debit transfers (amount) (0x0031)
        /// </summary>
        public const string TotalElectronicDebitTransfers = "Sas.AftDebitTransfers";

        /// <summary>
        ///     The bonus amount for deductible mode
        /// </summary>
        public const string BonusDeductibleAmount = "BonusDeductibleAmount";

        /// <summary>
        ///     The bonus amount for non deductible mode
        /// </summary>
        public const string BonusNonDeductibleAmount = "BonusNonDeductibleAmount";

        /// <summary>
        ///     Player Last Cashout Amount
        /// </summary>
        public const string EftPlayerLastCashoutAmount = "Sas.EftPlayerLastCashoutAmount";
    }
}
