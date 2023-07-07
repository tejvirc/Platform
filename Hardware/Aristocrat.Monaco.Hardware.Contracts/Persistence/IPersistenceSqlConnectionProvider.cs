namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System.Data.SQLite;

    /// <summary>
    /// Sql Connection provider
    /// </summary>
    public interface IPersistenceSqlConnectionProvider
    {
        /// <summary>
        /// Creates the connection to the database.
        /// </summary>
        /// <returns></returns>
        public SQLiteConnection CreateConnection();
    }
}
