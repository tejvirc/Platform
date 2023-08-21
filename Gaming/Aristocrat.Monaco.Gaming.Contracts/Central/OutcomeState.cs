namespace Aristocrat.Monaco.Gaming.Contracts.Central
{
    /// <summary>
    ///     Defines the game outcome status
    /// </summary>
    public enum OutcomeState
    {
        /// <summary>
        ///     The outcome has been requested
        /// </summary>
        Requested,

        /// <summary>
        ///     The outcome request failed
        /// </summary>
        Failed,

        /// <summary>
        ///     The outcome has been committed
        /// </summary>
        Committed,

        /// <summary>
        ///     The outcome has been acknowledged
        /// </summary>
        Acknowledged
    }
}