namespace Aristocrat.Monaco.Sas.Storage
{
    using System.IO;
    using Kernel;
    using Microsoft.Data.Sqlite;
    using Protocol.Common.Storage;

    /// <summary>
    ///     The default connection string resolver for the SAS persistence
    /// </summary>
    public class DefaultConnectionStringResolver : IConnectionStringResolver
    {
        private const string DataPath = @"/Data";
        private const string DatabaseFileName = @"Database_Sas.sqlite";
        private const string DatabasePassword = @"tk7tjBLQ8GpySFNZTHYD";

        private readonly string _connectionString;

        /// <summary>
        ///     Creates an instance of <see cref="DefaultConnectionStringResolver"/>.
        /// </summary>
        /// <param name="pathMapper">An instance of <see cref="IPathMapper"/></param>
        public DefaultConnectionStringResolver(IPathMapper pathMapper)
        {
            var dir = pathMapper.GetDirectory(DataPath);
            var path = Path.GetFullPath(dir.FullName);
            var sqlBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(path, DatabaseFileName),
                Password = DatabasePassword
            };

            _connectionString = sqlBuilder.ConnectionString;
        }

        /// <inheritdoc />
        public string Resolve()
        {
            return _connectionString;
        }
    }
}
