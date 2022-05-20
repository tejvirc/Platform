namespace Aristocrat.Monaco.Bingo.Services.Configuration
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Configuration constants that represent the system settings sent from the server
    /// </summary>
    [SuppressMessage(
        "ReSharper",
        "UnusedMember.Global",
        Justification = "These constants will be used as we properly recieve each of the server configuration settings")]
    public static class SystemConfigurationConstants
    {
        /// <summary>
        ///     Minimum jackpot value allowed to be set.
        /// </summary>
        public const string MinJackpotValue = "MinJackpotValue";

        /// <summary>
        ///     Minimum handpay value allowed to be set.
        /// </summary>
        public const string MinHandpayValue = "MinHandpayValue";

        /// <summary>
        ///     Jackpot Handling Strategy, interchangeable with Jackpot Strategy
        /// </summary>
        public const string JackpotHandlingStrategy = "JackpotHandlingStrategy";

        /// <summary>
        ///     Jackpot Amount Determination
        /// </summary>
        public const string JackpotAmountDetermination = "JackpotAmountDetermination";

        /// <summary>
        ///     Allows or prevents ticket reprint
        /// </summary>
        public const string TicketReprint = "TicketReprint";

        /// <summary>
        ///     Allows or prevents handpay receipt printing
        /// </summary>
        public const string HandpayReceipt = "HandpayReceipt";

        /// <summary>
        ///     Enables or disables Sas AFT functionality
        /// </summary>
        public const string SasAft = "SasAft";

        /// <summary>
        ///     Enables or disables Sas AFT bonusing functionality
        /// </summary>
        public const string AftBonusing = "AFTBonusing";

        /// <summary>
        ///     Enables or disables Sas Legacy Bonusing functionality
        /// </summary>
        public const string SasLegacyBonusing = "SASLegacyBonusing";

        /// <summary>
        ///     TransferWin2Host
        /// </summary>
        public const string TransferWin2Host = "TransferWin2Host";

        /// <summary>
        ///     Sets the transfer limit Transfer Limit
        /// </summary>
        public const string TransferLimit = "TransferLimit";

        /// <summary>
        ///     Barcode Width
        /// </summary>
        public const string BarcodeWidth = "BarcodeWidth";

        /// <summary>
        ///     Voucher Threshold
        /// </summary>
        public const string VoucherThreshold = "VoucherThreshold";

        /// <summary>
        ///     Bad Count Threshold
        /// </summary>
        public const string BadCountThreshold = "BadCountThreshold";

        /// <summary>
        ///     Sets the max voucher in value allowed
        /// </summary>
        public const string MaxVoucherIn = "MaxVoucherIn";

        /// <summary>
        ///     Binary File Receiver
        /// </summary>
        public const string BinaryFileReceiver = "BinaryFileReceiver";

        /// <summary>
        ///     Gives the NVRAM file location
        /// </summary>
        public const string NvramLocation = "NvramLocation";

        /// <summary>
        ///     Enables or disables the Cash Out Button
        /// </summary>
        public const string CashOutButton = "CashOutButton";

        /// <summary>
        ///     Credits Manager
        /// </summary>
        public const string CreditsStrategy = "CreditsManager";

        /// <summary>
        ///     Record Game Play, interchangeable with Capture Game Analytics
        /// </summary>
        public const string RecordGamePlay = "RecordGamePlay";

        /// <summary>
        ///     Capture Game Analytics, interchangeable with Record Game Play
        /// </summary>
        public const string CaptureGameAnalytics = "CaptureGameAnalytics";

        /// <summary>
        ///     Gen8 Max Voucher In
        /// </summary>
        public const string Gen8MaxVoucherIn = "Gen8MaxVoucherIn";

        /// <summary>
        ///     Gen8 Max Cash In
        /// </summary>
        public const string Gen8MaxCashIn = "Gen8MaxCashIn";

        /// <summary>
        ///     The Progressive Broadcast Timeout in seconds
        /// </summary>
        public const string ProgressiveBroadcastTimeout = "ProgressiveBroadcastTimeout";

        /// <summary>
        ///     Audible Alarm Setting
        /// </summary>
        public const string AudibleAlarmSetting = "AudibleAlarmSetting";
    }
}
