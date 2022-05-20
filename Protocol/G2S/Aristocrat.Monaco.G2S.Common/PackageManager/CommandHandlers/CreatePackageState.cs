namespace Aristocrat.Monaco.G2S.Common.PackageManager.CommandHandlers
{
    /// <summary>
    ///     Create package statutes
    /// </summary>
    public enum CreatePackageState
    {
        /// <summary>
        ///     The none
        /// </summary>
        None,

        /// <summary>
        ///     The created
        /// </summary>
        Created,

        /// <summary>
        ///     The duplicate package
        /// </summary>
        DuplicatePackage,

        /// <summary>
        ///     The module not exists
        /// </summary>
        ModuleNotExists
    }
}