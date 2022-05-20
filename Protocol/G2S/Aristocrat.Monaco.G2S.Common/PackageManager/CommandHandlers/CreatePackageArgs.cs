namespace Aristocrat.Monaco.G2S.Common.PackageManager.CommandHandlers
{
    using System;
    using Storage;
    using Data.Model;

    /// <summary>
    ///     Create package arguments
    /// </summary>
    public class CreatePackageArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CreatePackageArgs" /> class.
        /// </summary>
        /// <param name="packageLogEntity">The package log entity.</param>
        /// <param name="module">Module</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <param name="format">Compression format</param>
        public CreatePackageArgs(PackageLog packageLogEntity, Module module, bool overwrite, string format)
        {
            PackageLogEntity = packageLogEntity ?? throw new ArgumentNullException(nameof(packageLogEntity));
            ModuleEntity = module ?? throw new ArgumentNullException(nameof(module));
            Overwrite = overwrite;
            Format = format ?? throw new ArgumentNullException(nameof(format));
        }

        /// <summary>
        ///     Gets the package log entity.
        /// </summary>
        /// <value>
        ///     The package log entity.
        /// </value>
        public PackageLog PackageLogEntity { get; }

        /// <summary>
        ///     Gets the module entity.
        /// </summary>
        /// <value>
        ///     The module entity.
        /// </value>
        public Module ModuleEntity { get; }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="CreatePackageArgs" /> is overwrite.
        /// </summary>
        /// <value>
        ///     <c>true</c> if overwrite; otherwise, <c>false</c>.
        /// </value>
        public bool Overwrite { get; }

        /// <summary>
        ///     Gets compression format
        /// </summary>
        public string Format { get; }
    }
}