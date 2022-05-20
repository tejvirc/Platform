namespace Aristocrat.Monaco.Mgam.Common.Data
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
        ///     
        /// </summary>
        /// <param name="pathMapper"></param>
        public DefaultConnectionStringResolver(IPathMapper pathMapper)
        {
            var dir = pathMapper.GetDirectory(MgamConstants.DataPath);

            var path = Path.GetFullPath(dir.FullName);

            var sqlBuilder = new SqlConnectionStringBuilder
            {
                DataSource = Path.Combine(path, MgamConstants.DatabaseFileName),
                Password = MgamConstants.DatabasePassword
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
