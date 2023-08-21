namespace Aristocrat.Monaco.G2S.Common.GAT.Storage
{
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Gat component verification repository
    /// </summary>
    public class GatComponentVerificationRepository : BaseRepository<GatComponentVerification>,
        IGatComponentVerificationRepository
    {
        /// <inheritdoc />
        public IEnumerable<GatComponentVerification> GetByVerificationId(DbContext context, long requestId)
        {
            return Get(context, x => x.RequestId == requestId);
        }

        /// <inheritdoc />
        public GatComponentVerification GetByComponentIdAndVerificationId(
            DbContext context,
            string componentId,
            long requestId)
        {
            return context.Set<GatComponentVerification>()
                .FirstOrDefault(x => x.RequestId == requestId && x.ComponentId == componentId);
        }
    }
}