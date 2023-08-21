namespace Aristocrat.Monaco.G2S.Common.PackageManager.CommandHandlers
{
    using System;
    using Kernel.Contracts;
    using PackageManifest.Models;
    using Storage;

    /// <summary>
    ///     Install package arguments
    /// </summary>
    public class InstallPackageArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InstallPackageArgs" /> class.
        /// </summary>
        /// <param name="packageEntity">The package entity.</param>
        /// <param name="installPackageCallback">Install package callback</param>
        /// <param name="deleteAfter">Flag to delete package after install.</param>
        public InstallPackageArgs(
            Package packageEntity,
            Action<InstallPackageArgs> installPackageCallback,
            bool deleteAfter)
        {
            PackageEntity = packageEntity ?? throw new ArgumentNullException(nameof(packageEntity));
            InstallPackageCallback =
                installPackageCallback ?? throw new ArgumentNullException(nameof(installPackageCallback));
            DeleteAfter = deleteAfter;
        }

        /// <summary>
        ///     Gets the package entity.
        /// </summary>
        /// <value>
        ///     The package entity.
        /// </value>
        public Package PackageEntity { get; }

        /// <summary>
        ///     Gets or sets the package manifest
        /// </summary>
        public Image PackageManifest { get; set; }

        /// <summary>
        ///     Gets or sets the installation directory.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        ///     Gets or sets the installation directory size.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the package should be deleted after install.
        /// </summary>
        public bool DeleteAfter { get; }

        /// <summary>
        ///     Gets install package callback
        /// </summary>
        public Action<InstallPackageArgs> InstallPackageCallback { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not an active device was changed while installing or uninstalling
        /// </summary>
        public bool DeviceChanged { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not a shutdown is required after installing or uninstalling
        /// </summary>
        public ExitAction? ExitAction { get; set; }
    }
}