namespace Aristocrat.Monaco.Application.Drm
{
    /// <summary>
    ///     Defines a counter for a token
    /// </summary>
    public enum Counter
    {
        /// <summary>
        ///     Undefined counter
        /// </summary>
        None,

        /// <summary>
        ///     Defines the time remaining timer
        /// </summary>
        TimeRemaining,

        /// <summary>
        ///     Defines the maximum number of allocated licenses
        /// </summary>
        LicenseCount
    }
}