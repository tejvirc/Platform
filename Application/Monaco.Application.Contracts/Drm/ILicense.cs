namespace Aristocrat.Monaco.Application.Contracts.Drm
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Defines the license configuration
    /// </summary>
    public enum Configuration
    {
        /// <summary>
        ///     Standard license
        /// </summary>
        Standard,

        /// <summary>
        ///     VIP license
        /// </summary>
        Vip
    }

    /// <summary>
    ///     Provides details about a license
    /// </summary>
    [CLSCompliant(false)]
    public interface ILicense
    {
        /// <summary>
        ///     Gets the license identifier
        /// </summary>
        string Id { get; }

        /// <summary>
        ///     Gets the version of the license
        /// </summary>
        Version Version { get; }

        /// <summary>
        ///     Gets the configuration of the license
        /// </summary>
        Configuration Configuration { get; }

        /// <summary>
        ///     Gets the configured licenses
        /// </summary>
        IEnumerable<string> Licenses { get; }
    }
}