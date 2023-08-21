namespace Aristocrat.Monaco.G2S.Data.Packages
{
    using Common.Storage;
    using Model;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    ///     Repository interface for <see cref="PackageLog" />
    /// </summary>
    public interface IPackageLogRepository : IRepository<PackageLog>
    {
        /// <summary>
        ///     Gets Last Package Loge By PackageId
        /// </summary>
        /// <param name="context">DB context.</param>
        /// <param name="packageId">Package Id.</param>
        /// <returns>Package Log.</returns>
        PackageLog GetLastPackageLogeByPackageId(DbContext context, string packageId);
    }
}