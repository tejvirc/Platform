namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    /// <summary>
    ///     Defines the hard meter cashout state, which is dependent on the hard meter finishing
    ///     ticking to the new value we have set.
    /// </summary>
    public enum HardMeterOutState
    {
        /// <summary>
        ///     Pending hard meter ticking to the new value
        /// </summary>
        Pending,

        /// <summary>
        ///     Completed hard meter ticking to the new value
        /// </summary>
        Completed
    }
}