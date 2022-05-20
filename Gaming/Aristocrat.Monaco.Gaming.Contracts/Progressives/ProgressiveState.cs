namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    /// <summary>
    ///     Describes the current state of the progressive
    /// </summary>
    public enum ProgressiveState
    {
        /// <summary>
        ///     The initial state of the progressive
        /// </summary>
        Hit,

        /// <summary>
        ///     Pending
        /// </summary>
        Pending,

        /// <summary>
        ///     Committed
        /// </summary>
        Committed,

        /// <summary>
        ///     The progressive has been acknowledged by the host
        /// </summary>
        Acknowledged,

        /// <summary>
        ///     Failed
        /// </summary>
        Failed
    }
}