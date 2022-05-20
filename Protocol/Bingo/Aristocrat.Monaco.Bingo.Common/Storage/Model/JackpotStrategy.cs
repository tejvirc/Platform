namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System.ComponentModel;

    public enum JackpotStrategy
    {
        /// <summary>
        ///     An unknown jackpot strategy
        ///     This strategy should never be used and only occur when we have not received the configuration from the server
        /// </summary>
        [Description("Unknown")]
        Unknown = 0,

        /// <summary>
        ///     Strategy for crediting the machine with no jackpot handling
        /// </summary>
        [Description("Credit Jackpot Win")]
        CreditJackpotWin,

        /// <summary>
        ///     Strategy for locking up and paying the win amount to the credit meter when a jackpot occurs
        /// </summary>
        [Description("Lock Jackpot Win")]
        LockJackpotWin,

        /// <summary>
        ///     Strategy used for forcing a handpay for jackpot wins with printing a receipt
        /// </summary>
        [Description("Handpay Jackpot Win")]
        HandpayJackpotWin,

        /// <summary>
        ///     Strategy used for forcing a handpay for jackpot wins without printing a receipt
        /// </summary>
        [Description("Lock Jackpot Win No History")]
        LockJackpotWinNoHistory,

        /// <summary>
        ///     The maximum value for JackpotStrategy
        ///     This strategy should never be used
        /// </summary>
        MaxJackpotStrategy
    }
}