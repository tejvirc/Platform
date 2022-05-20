namespace Aristocrat.Monaco.G2S.Common.PackageManager.Storage
{
    using System.Data.Entity;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Base interface for package repository.
    /// </summary>
    public interface IPackageRepository : IRepository<Package>
    {
        /// <summary>
        ///     Gets package instance by specified package Id.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="packageId">Package Id.</param>
        /// <returns>Returns package instance or null.</returns>
        Package GetPackageByPackageId(DbContext context, string packageId);
    }
}