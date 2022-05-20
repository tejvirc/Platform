namespace Aristocrat.Monaco.Application.Contracts.Drm
{
    using System;

    /// <summary>
    ///     Provides a mechanism to determine support for developer mode
    /// </summary>
    [CLSCompliant(false)]
    public interface ILicenseMode
    {
        /// <summary>
        ///     Gets a value indicating whether or not licensing is in developer mode
        /// </summary>
        bool IsDeveloper { get; }
    }
}