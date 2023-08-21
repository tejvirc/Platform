namespace Aristocrat.Monaco.G2S.Common.GAT.Storage
{
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Gat verification request repository interface
    /// </summary>
    public interface IGatVerificationRequestRepository : IRepository<GatVerificationRequest>
    {
        /// <summary>
        ///     Gets the maximum last sequence.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <returns>Max last sequence.</returns>
        long GetMaxLastSequence(DbContext context);

        /// <summary>
        ///     Gets the by verification identifier.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="verificationId">The verification identifier.</param>
        /// <returns>Gat verification request.</returns>
        GatVerificationRequest GetByVerificationId(DbContext context, long verificationId);

        /// <summary>
        ///     Gets the by transaction identifier.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <returns>GAT verification request.</returns>
        GatVerificationRequest GetByTransactionId(DbContext context, long transactionId);

        /// <summary>
        ///     Gets the verification requests.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <returns>GAT verification requests.</returns>
        IEnumerable<GatVerificationRequest> GetVerificationRequests(DbContext context);
    }
}