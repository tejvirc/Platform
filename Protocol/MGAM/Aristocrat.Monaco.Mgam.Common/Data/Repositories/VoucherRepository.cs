namespace Aristocrat.Monaco.Mgam.Common.Data.Repositories
{
    using System.Linq;
    using Models;
    using Protocol.Common.Storage.Repositories;

    /// <summary>
    ///     Voucher Repository
    /// </summary>
    public static class VoucherRepository
    {
        private static readonly object VoucherLock = new object();

        /// <summary>
        ///     Adds Voucher
        /// </summary>
        public static void AddVoucher(this IRepository<Voucher> repository, Voucher voucher)
        {
            lock (VoucherLock)
            {
                var oldVoucher = repository
                    .Queryable().SingleOrDefault();

                if (oldVoucher != null)
                {
                    if (oldVoucher.OfflineReason != VoucherOutOfflineReason.None)
                    {
                        voucher.OfflineReason = oldVoucher.OfflineReason;
                    }

                    repository.Delete(oldVoucher);
                }

                repository.Add(voucher);
            }
        }
    }
}
