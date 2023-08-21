namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;

    /// <summary>
    ///     Provides a mechanism to get product release information
    /// </summary>
    public interface IReleaseInfo
    {
        /// <summary>
        ///     Gets the version number
        /// </summary>
        string Version { get; }

        /// <summary>
        ///     Gets the release date and time
        /// </summary>
        DateTime ReleaseDate { get; }

        /// <summary>
        ///     Gets the install DateTime of the game.
        /// </summary>
        DateTime InstallDate { get; }

        /// <summary>
        ///     Gets a value indicating whether the game was upgraded from a previous version
        /// </summary>
        bool Upgraded { get; }
    }
}
