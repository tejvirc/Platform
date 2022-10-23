namespace Aristocrat.Monaco.G2S.Data.CommConfig
{
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using Common.Storage;
    using Model;

    /// <summary>
    ///     Comm Change Log Repository
    /// </summary>
    public class CommChangeLogRepository : BaseRepository<CommChangeLog>, ICommChangeLogRepository
    {
        private readonly List<ChangeStatus> _pendingStatuses = new List<ChangeStatus>
        {
            ChangeStatus.Pending, ChangeStatus.Authorized
        };

        /// <inheritdoc />
        public CommChangeLog GetByTransactionId(DbContext context, long transactionId)
        {
            return Get(context, x => x.TransactionId == transactionId)
                .Include(x => x.AuthorizeItems)
                .SingleOrDefault();
        }

        /// <inheritdoc />
        public CommChangeLog GetPendingByTransactionId(DbContext context, long transactionId)
        {
            return
                Get(context, x => x.TransactionId == transactionId && _pendingStatuses.Contains(x.ChangeStatus))
                    .Include(x => x.AuthorizeItems)
                    .SingleOrDefault();
        }

        /// <inheritdoc />
        public IQueryable<CommChangeLog> GetPending(DbContext context)
        {
            return Get(context, x => _pendingStatuses.Contains(x.ChangeStatus))
                .Include(x => x.AuthorizeItems);
        }
    }
}