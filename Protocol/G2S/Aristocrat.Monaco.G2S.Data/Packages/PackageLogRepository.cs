namespace Aristocrat.Monaco.G2S.Data.Packages
{
    using Common.Storage;
    using Model;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;

    /// <summary>
    ///     PackageLogRepository
    /// </summary>
    public class PackageLogRepository : BaseRepository<PackageLog>, IPackageLogRepository
    {
        /// <inheritdoc />
        public PackageLog GetLastPackageLogeByPackageId(DbContext context, string packageId)
        {
            return context.Set<PackageLog>().Where(a => a.PackageId.Equals(packageId))
                .OrderByDescending(l => l.Id).FirstOrDefault();
        }
    }
}