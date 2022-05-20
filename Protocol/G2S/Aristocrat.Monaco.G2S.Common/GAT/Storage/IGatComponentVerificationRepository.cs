namespace Aristocrat.Monaco.G2S.Common.GAT.Storage
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using Monaco.Common.Storage;

    /// <summary>
    ///     GAT component verification repository interface
    /// </summary>
    public interface IGatComponentVerificationRepository : IRepository<GatComponentVerification>
    {
        /// <summary>
        ///     Gets the by verification request identifier.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="requestId">The verification request identifier.</param>
        /// <returns>List of component verification.</returns>
        IEnumerable<GatComponentVerification> GetByVerificationId(DbContext context, long requestId);

        /// <summary>
        ///     Gets the by component Id and verification request identifier.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="componentId">The component identifier.</param>
        /// <param name="requestId">The verification request identifier.</param>
        /// <returns>GatComponentVerificationEntity</returns>
        GatComponentVerification GetByComponentIdAndVerificationId(
            DbContext context,
            string componentId,
            long requestId);
    }
}