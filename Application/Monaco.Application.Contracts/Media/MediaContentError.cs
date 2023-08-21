namespace Aristocrat.Monaco.Application.Contracts.Media
{
    /// <summary>
    ///     Content error condition
    /// </summary>
    public enum MediaContentError
    {
        /// <summary>
        ///     No error
        /// </summary>
        None = 0,

        /// <summary>
        ///     Content transfer failed (URI not accessible by EGM)
        /// </summary>
        TransferFailed = 1,

        /// <summary>
        ///     File size exceeds system capability
        /// </summary>
        FileSizeLimitation = 2,

        /// <summary>
        ///     Error during content execution
        /// </summary>
        RuntimeError = 3,

        /// <summary>
        ///     Resource limits exceeded
        /// </summary>
        ResourceLimitation = 4,

        /// <summary>
        ///     Invalid MD access token
        /// </summary>
        InvalidMdAccessToken = 5,

        /// <summary>
        ///     EMDI connectoin required/EMDI reconnect timer expired
        /// </summary>
        EmdiConnectionError = 6,
    }
}