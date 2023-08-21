namespace Aristocrat.Monaco.Application.Contracts
{
    /// <summary>
    ///     Represents an installed game package
    /// </summary>
    public interface IInstalledPackage
    {
        /// <summary>
        ///     Gets the unique package identifier
        /// </summary>
        string PackageId { get; }

        /// <summary>
        ///     Gets the path to the package file
        /// </summary>
        string Package { get; }
    }
}
