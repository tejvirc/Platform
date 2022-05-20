namespace Aristocrat.Monaco.Application.Contracts.Drm
{
    using System;
    using System.IO;

    /// <summary>
    ///     Provides a mechanism to interact with digital rights management
    /// </summary>
    [CLSCompliant(false)]
    public interface IDigitalRights : ILicenseMode, IJurisdiction, IDisposable
    {
        /// <summary>
        ///     Gets a value indicating whether or not there is a critical error due to an error with the protection module 
        /// </summary>
        bool Disabled { get; }

        /// <summary>
        ///     Gets the current license
        /// </summary>
        ILicense License { get; }

        /// <summary>
        ///     Gets the time remaining for the current license
        /// </summary>
        TimeSpan TimeRemaining { get; }

        /// <summary>
        ///     Gets the maximum number of licensed products
        /// </summary>
        int LicenseCount { get; }

        /// <summary>
        ///     Determines if the specified media is licensed
        /// </summary>
        /// <param name="media">The media to validate</param>
        /// <returns>true if licensed, otherwise false</returns>
        bool IsLicensed(FileInfo media);
    }
}