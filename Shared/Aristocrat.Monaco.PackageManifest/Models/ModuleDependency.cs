namespace Aristocrat.Monaco.PackageManifest.Models
{
    /// <summary>
    ///     Defines a module that must be present or absent
    /// </summary>
    public class ModuleDependency : Dependency
    {
        /// <summary>
        ///     Defines the pattern that is used to check for a module
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the dependency should be negated
        /// </summary>
        public bool Not { get; set; }
    }
}