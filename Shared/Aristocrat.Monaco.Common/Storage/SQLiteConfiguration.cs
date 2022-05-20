namespace Aristocrat.Monaco.Common.Storage
{
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Core.Common;
    using System.Data.SQLite;
    using System.Data.SQLite.EF6;
    using System.Linq;

    /// <summary>
    ///     SQLite configuration class
    /// </summary>
    public class SQLiteConfiguration : DbConfiguration
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SQLiteConfiguration" /> class.
        /// </summary>
        public SQLiteConfiguration()
        {
            // Jump thru some hoops since we don't have an app.config
            var assemblyName = typeof(SQLiteProviderFactory).Assembly.GetName().Name;

            RegisterDbProviderFactories(assemblyName);
            SetProviderFactory(assemblyName, SQLiteFactory.Instance);
            SetProviderFactory(assemblyName, SQLiteProviderFactory.Instance);
            SetProviderServices(
                assemblyName,
                (DbProviderServices)SQLiteProviderFactory.Instance.GetService(typeof(DbProviderServices)));
        }

        private static void RegisterDbProviderFactories(string assemblyName)
        {
            if (!(ConfigurationManager.GetSection("system.data") is DataSet dataSet))
            {
                return;
            }

            var dbProviderFactoriesDataTable = dataSet.Tables.OfType<DataTable>()
                .First(x => x.TableName == nameof(DbProviderFactories));

            var dataRow = dbProviderFactoriesDataTable.Rows.OfType<DataRow>()
                .FirstOrDefault(x => x.ItemArray[2].ToString() == assemblyName);

            if (dataRow != null)
            {
                dbProviderFactoriesDataTable.Rows.Remove(dataRow);
            }

            dbProviderFactoriesDataTable.Rows.Add(
                "SQLite Data Provider (Entity Framework 6)",
                ".NET Framework Data Provider for SQLite (Entity Framework 6)",
                assemblyName,
                typeof(SQLiteProviderFactory).AssemblyQualifiedName);
        }
    }
}