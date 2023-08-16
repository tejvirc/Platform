namespace Aristocrat.Monaco.G2S.Common.PackageManager.Storage
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Package error repository implementation.
    /// </summary>
    public class PackageErrorRepository : BaseRepository<PackageError>, IPackageErrorRepository
    {
        /// <inheritdoc />
        public IEnumerable<PackageError> GetByPackageId(DbContext context, string packageId)
        {
            return context.Set<PackageError>()
                .Where(x => string.Compare(x.PackageId, packageId, StringComparison.InvariantCultureIgnoreCase) == 0)
                .ToList();
        }

        /// <inheritdoc />
        public void DeleteByPackageId(DbContext context, string packageId)
        {
            var foundEntities = context.Set<PackageError>()
                .Where(x => string.Compare(x.PackageId, packageId, StringComparison.InvariantCultureIgnoreCase) == 0);
            context.Set<PackageError>().RemoveRange(foundEntities);
            context.SaveChanges();
        }
    }
}