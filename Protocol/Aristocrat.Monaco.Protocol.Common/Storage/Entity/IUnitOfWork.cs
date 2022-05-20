namespace Aristocrat.Monaco.Protocol.Common.Storage.Entity
{
    using System;
    using System.Data;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Encapsulates database access.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        ///     Saves changes to the database.
        /// </summary>
        /// <returns>Returns the count of rows effected by the query.</returns>
        int SaveChanges();

        /// <summary>
        ///     Gets a reference to repository instance.
        /// </summary>
        /// <typeparam name="TEntity">The Entity data type for the repository.</typeparam>
        /// <returns>Repository instance.</returns>
        Repositories.IRepository<TEntity> Repository<TEntity>()
            where TEntity : BaseEntity;

        /// <summary>
        ///     Gets or sets the command time out.
        /// </summary>
        int? CommandTimeout { get; set; }

        /// <summary>
        ///     Starts a database transaction.
        /// </summary>
        /// <param name="isolationLevel"></param>
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);

        /// <summary>
        ///     Commits changes to the database from the beginning of a transaction.
        /// </summary>
        /// <returns></returns>
        void Commit();

        /// <summary>
        ///     Discards changes to the database from the beginning of a transaction.
        /// </summary>
        void Rollback();
    }
}
