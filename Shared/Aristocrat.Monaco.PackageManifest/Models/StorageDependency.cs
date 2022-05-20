namespace Aristocrat.Monaco.PackageManifest.Models
{
    /// <summary>
    ///     Defines a storage dependency for a package
    /// </summary>
    public class StorageDependency : Dependency
    {
        /// <summary>
        ///     Gets or sets the storage type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        ///     Gets or sets the type of application for which the storage will be used
        /// </summary>
        public string Application { get; set; }

        /// <summary>
        ///     Gets or sets the minimum amount of free storage of the specified storage type that must be available
        /// </summary>
        public long Size { get; set; }
    }
}