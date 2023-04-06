namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using System.ComponentModel;

    /// <summary>
    ///     Defines the handpay state
    /// </summary>
    public enum HardMeterOutState
    {
        /// <summary>
        ///     Pending Hard Meter tick finish
        /// </summary>
        [Description("PendingHardMeterComplete")]
        PendingHardMeterComplete,

        /// <summary>
        ///     Completed
        /// </summary>
        [Description("Completed")]
        Completed
    }
}