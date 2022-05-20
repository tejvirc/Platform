namespace Aristocrat.Monaco.Accounting.Contracts.Handpay
{
    using System.ComponentModel;

    /// <summary>
    ///     Defines the handpay types
    /// </summary>
    public enum HandpayType
    {
        /// <summary>
        ///     Game win
        /// </summary>
        [Description("Game Win")]
        GameWin,

        /// <summary>
        ///     Bonus payment
        /// </summary>
        [Description("Bonus Pay")]
        BonusPay,

        /// <summary>
        ///     Cancel credit
        /// </summary>
        [Description("Cancelled Credit")]
        CancelCredit
    }
}
