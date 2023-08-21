namespace Aristocrat.Monaco.Accounting.Contracts.Handpay
{
    using System.ComponentModel;

    /// <summary>
    ///     Defines the large win handpay reset method.
    /// </summary>
    public enum LargeWinHandpayResetMethod
    {
        /// <summary>
        ///     Method used to handpay large win is pay by hand.
        /// </summary>
        [Description("Pay by Hand")]
        PayByHand = 0,

        /// <summary>
        ///     Method used to handpay large win is pay by menu selection.
        /// </summary>
        [Description("Pay by Menu Selection")]
        PayByMenuSelection,

        /// <summary>
        ///     Method used to handpay large win is pay by 1 key SAS.
        /// </summary>
        [Description("Pay by Host System")]
        PayBy1HostSystem
    }
}