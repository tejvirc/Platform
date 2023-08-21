namespace Aristocrat.Monaco.G2S.Common.GAT.Storage
{
    /// <summary>
    ///     Component verification statutes
    /// </summary>
    public enum ComponentVerificationState
    {
        /// <summary>
        ///     The none
        /// </summary>
        None = 0,

        /// <summary>
        ///     The queued
        /// </summary>
        Queued = 1,

        /// <summary>
        ///     The in process
        /// </summary>
        InProcess = 2,

        /// <summary>
        ///     The complete
        /// </summary>
        Complete = 3,

        /// <summary>
        ///     The error
        /// </summary>
        Error = 4,

        /// <summary>
        ///     The passed
        /// </summary>
        Passed = 5,

        /// <summary>
        ///     The failed
        /// </summary>
        Failed = 6
    }
}