namespace Aristocrat.Monaco.Protocol.Common.Installer
{
    using System;
    using Monaco.Common.Exceptions;
    using Kernel;
    using Kernel.Contracts;
    using PackageManifest.Models;

    /// <summary>
    ///     Provides a mechanism install cabinet components.
    /// </summary>
    public interface IInstallerService : IService
    {
        /// <summary>
        ///     Install software package.
        /// </summary>
        /// <param name="packageId">Software package Id.</param>
        /// <param name="updateAction">Install update action.</param>
        /// <returns>Image manifest, path, size, device changed, exit action.</returns>
        (Image manifest, string path, long size, bool deviceChanged, ExitAction? action) InstallPackage(
            string packageId,
            Action updateAction = null);

        /// <summary>
        ///     Validate software packages for installation.
        /// </summary>
        /// <param name="filePath">File of the software pending installation.</param>
        /// <returns>True if valid.</returns>
        bool ValidateSoftwarePackage(string filePath);

        /// <summary>
        ///     Search and reads manifest from directory that contains unpacked package.
        /// </summary>
        /// <param name="packageId">Package identifier</param>
        /// <param name="path">Manifest path.</param>
        /// <returns>Returns manifest instance.</returns>
        /// <exception cref="CommandException">Throws an exception in case manifest file was not found.</exception>
        Image ReadManifest(string packageId, string path = default(string));

        /// <summary>
        ///     Creates an upload-able software package.
        /// </summary>
        /// <param name="packageId">Software package to create.</param>
        /// <param name="overwrite">Overwrite existing package.</param>
        /// <param name="format">Archive format.</param>
        /// <returns>Size and name of the package.</returns>
        (long size, string name)
            BundleSoftwarePackage(string packageId, bool overwrite, string format = null);

        /// <summary>
        ///     Uninstall software package.
        /// </summary>
        /// <param name="packageId">Package Id.</param>
        /// <param name="uninstalledAction">Action to preform after the uninstall completes.</param>
        /// <returns>Manifest and device changed.</returns>
        (Image manifest, bool deviceChanged) UninstallSoftwarePackage(
            string packageId,
            Action<string[]> uninstalledAction = null);

        /// <summary>
        ///     Software package to delete.
        /// </summary>
        /// <param name="packageId">Package Id.</param>
        void DeleteSoftwarePackage(string packageId);
    }
}