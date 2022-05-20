namespace Aristocrat.Monaco.Common.Storage
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;

    /// <summary>
    ///     Provides a mechanism to create a Monaco DB Context
    /// </summary>
    public interface IMonacoContextFactory : IDbContextFactory<DbContext>
    {
        /// <summary>
        ///     Creates a new instance of a derived System.Data.Entity.DbContext type with locking preventing any other database
        ///     reads or writes
        /// </summary>
        /// <returns>An instance of TContext</returns>
        DbContext Lock();

        /// <summary>
        ///     Releases the exclusive lock obtained by a call to Lock
        /// </summary>
        void Release();
    }
}