namespace Aristocrat.Monaco.G2S.Data.Model
{
    /// <summary>
    ///     List of supported transfer states.
    /// </summary>
    public enum TransferState
    {
        /// <summary>
        ///     The pending
        /// </summary>
        Pending,

        /// <summary>
        ///     The in progress
        /// </summary>
        InProgress,

        /// <summary>
        ///     The completed
        /// </summary>
        Completed,

        /// <summary>
        ///     Validated
        /// </summary>
        Validated,

        /// <summary>
        ///     The failed
        /// </summary>
        Failed
    }
}