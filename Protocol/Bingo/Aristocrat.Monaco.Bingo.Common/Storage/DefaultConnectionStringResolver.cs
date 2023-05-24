namespace Aristocrat.Monaco.Bingo.Common.Storage
{
    using System.IO;
    using Kernel;
    using Microsoft.Data.Sqlite;
    using Protocol.Common.Storage;

    public class DefaultConnectionStringResolver : IConnectionStringResolver
    {
        private const string DataPath = @"/Data";
        private const string DatabaseFileName = @"Database_Bingo.sqlite";

        private readonly string _connectionString;

        public DefaultConnectionStringResolver(IPathMapper pathMapper)
        {
            var dir = pathMapper.GetDirectory(DataPath);
            var path = Path.GetFullPath(dir.FullName);
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(path, DatabaseFileName),
                Pooling = true,
                DefaultTimeout = 15,
                Password = "tk7tjBLQ8GpySFNZTHYD"
            };
            _connectionString = builder.ConnectionString;
        }
        
        public string Resolve()
        {
            return _connectionString;
        }
    }
}
