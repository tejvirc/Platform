namespace Aristocrat.Monaco.Bingo.Common.Storage
{
    using System.Data.SqlClient;
    using System.IO;
    using Kernel;
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
            var sqlBuilder = new SqlConnectionStringBuilder
            {
                DataSource = Path.Combine(path, DatabaseFileName)
            };

            _connectionString = sqlBuilder.ConnectionString;
        }
        
        public string Resolve()
        {
            return _connectionString;
        }
    }
}
