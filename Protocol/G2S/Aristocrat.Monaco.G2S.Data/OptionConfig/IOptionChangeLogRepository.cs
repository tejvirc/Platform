namespace Aristocrat.Monaco.G2S.Data.OptionConfig
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using Common.Storage;

    /// <summary>
    ///     Repository interface for OptionConfigChangeLog.
    /// </summary>
    public interface IOptionChangeLogRepository : IRepository<OptionChangeLog>
    {
        /// <summary>
        ///     Gets the pending change logs.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <returns>Option change logs with pending status</returns>
        IQueryable<OptionChangeLog> GetPendingChangeLogs(DbContext context);

        /// <summary>
        ///     Gets the by transaction identifier.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <returns>Pending change log or null if it is not exists</returns>
        OptionChangeLog GetByTransactionId(DbContext context, long transactionId);

        /// <summary>
        ///     Gets the pending by transaction identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <returns>Option change log by transactionId.</returns>
        OptionChangeLog GetPendingByTransactionId(DbContext context, long transactionId);
    }
}