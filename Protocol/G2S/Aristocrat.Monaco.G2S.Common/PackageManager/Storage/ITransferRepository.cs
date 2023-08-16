namespace Aristocrat.Monaco.G2S.Common.PackageManager.Storage
{
    using Microsoft.EntityFrameworkCore;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Base interface for transfer entity repository.
    /// </summary>
    public interface ITransferRepository : IRepository<TransferEntity>
    {
        /// <summary>
        ///     Gets transfer instance by specified package Id.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="packageId">Package Id.</param>
        /// <returns>Returns package instance or null.</returns>
        TransferEntity GetByPackageId(DbContext context, string packageId);
    }
}