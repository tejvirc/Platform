namespace Aristocrat.Monaco.G2S.Common.PackageManager.Storage
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Repository implementation for package entity.
    /// </summary>
    public class PackageRepository : BaseRepository<Package>, IPackageRepository
    {
        /// <inheritdoc />
        public Package GetPackageByPackageId(DbContext context, string packageId)
        {
            return context.Set<Package>()
                .FirstOrDefault(
                    x => string.Compare(x.PackageId, packageId, StringComparison.InvariantCultureIgnoreCase) == 0);
        }
    }
}