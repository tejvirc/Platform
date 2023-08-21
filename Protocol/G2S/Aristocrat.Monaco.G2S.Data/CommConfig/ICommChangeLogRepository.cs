namespace Aristocrat.Monaco.G2S.Data.CommConfig
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using Common.Storage;

    /// <summary>
    ///     Repository interface for OptionConfigChangeLog.
    /// </summary>
    public interface ICommChangeLogRepository : IRepository<CommChangeLog>
    {
        /// <summary>
        ///     Gets the by transaction identifier.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <returns>Comm change log by transactionId</returns>
        CommChangeLog GetByTransactionId(DbContext context, long transactionId);

        /// <summary>
        ///     Gets the pending change log by the transaction identifier.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <returns>Comm change log by transactionId.</returns>
        CommChangeLog GetPendingByTransactionId(DbContext context, long transactionId);

        /// <summary>
        ///     Gets the pending change logs.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <returns>Pending change logs</returns>
        IQueryable<CommChangeLog> GetPending(DbContext context);
    }
}