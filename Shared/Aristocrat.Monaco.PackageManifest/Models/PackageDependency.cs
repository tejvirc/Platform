namespace Aristocrat.Monaco.PackageManifest.Models
{
    using System.Collections.Generic;

    /// <summary>
    ///     Defines the logical operators for a package dependency
    /// </summary>
    public enum Operator
    {
        /// <summary>
        ///     Logical "and" operator
        /// </summary>
        And,

        /// <summary>
        ///     Logical "or" operator
        /// </summary>
        Or
    }

    /// <summary>
    ///     Defines the dependencies for a package
    /// </summary>
    public class PackageDependency
    {
        /// <summary>
        ///     Gets or sets the operator
        /// </summary>
        public Operator Operator { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the dependency should be negated
        /// </summary>
        public bool Not { get; set; }

        /// <summary>
        ///     Gets or set the package dependencies
        /// </summary>
        public IEnumerable<Dependency> Dependencies { get; set; }
    }
}