namespace Aristocrat.Monaco.G2S.Common.GAT.Storage
{
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using Monaco.Common.Storage;

    /// <summary>
    ///     An implementation of IGatVerificationRequestRepository
    /// </summary>
    public class GatVerificationRequestRepository : BaseRepository<GatVerificationRequest>,
        IGatVerificationRequestRepository
    {
        /// <inheritdoc />
        public long GetMaxLastSequence(DbContext context)
        {
            return context.Set<GatVerificationRequest>().AsQueryable().DefaultIfEmpty().Max(x => x.Id);
        }

        /// <inheritdoc />
        public GatVerificationRequest GetByVerificationId(DbContext context, long verificationId)
        {
            return context.Set<GatVerificationRequest>()
                .Include(v => v.ComponentVerifications)
                .SingleOrDefault(x => x.VerificationId == verificationId);
        }

        /// <inheritdoc />
        public GatVerificationRequest GetByTransactionId(DbContext context, long transactionId)
        {
            return context.Set<GatVerificationRequest>()
                .Include(v => v.ComponentVerifications)
                .SingleOrDefault(x => x.TransactionId == transactionId);
        }

        /// <inheritdoc />
        public IEnumerable<GatVerificationRequest> GetVerificationRequests(DbContext context)
        {
            return context.Set<GatVerificationRequest>().Include(v => v.ComponentVerifications);
        }

        /// <inheritdoc />
        public override IQueryable<GatVerificationRequest> GetRange(
            DbContext context,
            long lastSequence,
            long totalEntries)
        {
            return base.GetRange(context, lastSequence, totalEntries).Include(x => x.ComponentVerifications);
        }
    }
}