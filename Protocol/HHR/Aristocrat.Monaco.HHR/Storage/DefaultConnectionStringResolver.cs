namespace Aristocrat.Monaco.Hhr.Storage
{
    using System.Data.SqlClient;
    using System.IO;
    using Kernel;
    using Protocol.Common.Storage;

    /// <summary>
    ///     
    /// </summary>
    public class DefaultConnectionStringResolver : IConnectionStringResolver
    {
        private readonly string _connectionString;
        /// <summary>
        ///     Path lookup of the database folder
        /// </summary>
        private const string DataPath = @"/Data";

        /// <summary>
        ///     Database file name
        /// </summary>
        private const string DatabaseFileName = @"Database_HHR.sqlite";

        /// <summary>
        ///     Database password
        /// </summary>
        private const string DatabasePassword = @"tk7tjBLQ8GpySFNZTHYD";

        /// <summary>
        ///     
        /// </summary>
        /// <param name="pathMapper"></param>
        public DefaultConnectionStringResolver(IPathMapper pathMapper)
        {
            var dir = pathMapper.GetDirectory(DataPath);

            var path = Path.GetFullPath(dir.FullName);

            var sqlBuilder = new SqlConnectionStringBuilder
            {
                DataSource = Path.Combine(path, DatabaseFileName)
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
