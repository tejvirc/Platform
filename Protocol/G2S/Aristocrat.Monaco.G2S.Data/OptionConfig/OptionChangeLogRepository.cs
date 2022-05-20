namespace Aristocrat.Monaco.G2S.Data.OptionConfig
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Common.Storage;
    using Model;

    /// <summary>
    ///     Option Change Log Repository
    /// </summary>
    public class OptionChangeLogRepository : BaseRepository<OptionChangeLog>, IOptionChangeLogRepository
    {
        private readonly List<ChangeStatus> _pendingStatuses = new List<ChangeStatus>
        {
            ChangeStatus.Pending,
            ChangeStatus.Authorized
        };

        /// <inheritdoc />
        public override IQueryable<OptionChangeLog> GetRange(DbContext context, long lastSequence, long totalEntries)
        {
            return base.GetRange(context, lastSequence, totalEntries).Include(e => e.AuthorizeItems).AsQueryable();
        }

        /// <inheritdoc />
        public IQueryable<OptionChangeLog> GetPendingChangeLogs(DbContext context)
        {
            return Get(context, x => x.ChangeStatus == ChangeStatus.Pending);
        }

        /// <inheritdoc />
        public OptionChangeLog GetByTransactionId(DbContext context, long transactionId)
        {
            return Get(context, x => x.TransactionId == transactionId)
                .Include(x => x.AuthorizeItems)
                .SingleOrDefault();
        }

        /// <inheritdoc />
        public OptionChangeLog GetPendingByTransactionId(DbContext context, long transactionId)
        {
            return
                Get(context, x => x.TransactionId == transactionId && _pendingStatuses.Contains(x.ChangeStatus))
                    .Include(x => x.AuthorizeItems)
                    .SingleOrDefault();
        }
    }
}