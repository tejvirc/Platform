namespace Aristocrat.Monaco.PackageManifest.Models
{
    /// <summary>
    ///     Describes a package in the manifest
    /// </summary>
    public class ManifestPackage
    {
        /// <summary>
        ///     Gets or sets the Package identifier, assigned by the manufacturer
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        ///     Gets or sets the Module identifier, assigned by the manufacturer.
        /// </summary>
        public string ModuleId { get; set; }

        /// <summary>
        ///     Gets or sets the Release number of the package
        /// </summary>
        public string ReleaseNumber { get; set; }

        /// <summary>
        ///     Gets or sets the file name of the package
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        ///     Gets or sets the anticipated disk space size in bytes of the package when installed
        /// </summary>
        public long PackageSize { get; set; }

        /// <summary>
        ///     Gets or sets the package dependency
        /// </summary>
        public PackageDependency Dependency { get; set; }
    }
}