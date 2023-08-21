namespace Aristocrat.Monaco.G2S.Common.PackageManager.Storage
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Base implementation for transfer entity repository.
    /// </summary>
    public class TransferRepository : BaseRepository<TransferEntity>, ITransferRepository
    {
        /// <inheritdoc />
        public TransferEntity GetByPackageId(DbContext context, string packageId)
        {
            var set = context.Set<TransferEntity>().Where(
                x => string.Compare(x.PackageId, packageId, StringComparison.InvariantCultureIgnoreCase) == 0);

            if (set.Any())
            {
                return set.OrderByDescending(a => a.Id).FirstOrDefault();
            }

            return null;
        }
    }
}