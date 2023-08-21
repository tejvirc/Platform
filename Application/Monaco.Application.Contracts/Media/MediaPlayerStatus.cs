namespace Aristocrat.Monaco.Application.Contracts.Media
{
    using System;

    /// <summary>
    ///     Indicates the states of the media player
    /// </summary>
    [Flags]
    public enum MediaPlayerStatus
    {
        /// <summary>
        ///     Indicates the media player is enabled
        /// </summary>
        None = 0,

        /// <summary>
        ///     Indicates the media player is disabled by the System/EGM
        /// </summary>
        DisabledBySystem = 1,

        /// <summary>
        ///     Indicates the media player is disabled by the backend
        /// </summary>
        DisabledByBackend = 2
    }
}