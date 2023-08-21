namespace Aristocrat.Monaco.Application.Contracts.Media
{
    /// <summary>
    ///     The content state
    /// </summary>
    public enum MediaState
    {
        /// <summary>
        ///     Pending
        /// </summary>
        Pending,

        /// <summary>
        ///     Loaded
        /// </summary>
        Loaded,

        /// <summary>
        ///     Executing
        /// </summary>
        Executing,

        /// <summary>
        ///     Released
        /// </summary>
        Released,

        /// <summary>
        ///     Error
        /// </summary>
        Error
    }
}