namespace Aristocrat.Monaco.Mgam.Services.Attributes
{
    /// <summary>
    ///     The options for syncing the server on updates.
    /// </summary>
    public enum AttributeSyncBehavior
    {
        /// <summary>
        ///     The update should only be made locally. This should be used when the
        ///     value being set was received from the server.
        /// </summary>
        LocalOnly = 0,

        /// <summary>
        ///     The update should be made locally and sent to the server.
        /// </summary>
        LocalAndServer = 1,

        /// <summary>
        ///     The update source is from the server
        /// </summary>
        ServerSource = 2
    }
}