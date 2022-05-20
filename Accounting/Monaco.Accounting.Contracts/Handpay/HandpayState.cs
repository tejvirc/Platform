namespace Aristocrat.Monaco.Accounting.Contracts.Handpay
{
    using System.ComponentModel;

    /// <summary>
    ///     Defines the handpay state
    /// </summary>
    public enum HandpayState
    {
        /// <summary>
        ///     Handpay requested
        /// </summary>
        [Description("Requested")]
        Requested,

        /// <summary>
        ///     Handpay pending
        /// </summary>
        [Description("Pending")]
        Pending,

        /// <summary>
        ///     Handpay committed
        /// </summary>
        [Description("Committed")]
        Committed,

        /// <summary>
        ///     Handpay acknowledged
        /// </summary>
        [Description("Acknowledged")]
        Acknowledged
    }
}