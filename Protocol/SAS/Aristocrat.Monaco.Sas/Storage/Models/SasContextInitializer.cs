namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System.Data.Entity;
    using SQLite.CodeFirst;

    /// <summary>
    ///     The context initializer for SAS
    /// </summary>
    public class SasContextInitializer : SqliteCreateDatabaseIfNotExists<SasContext>
    {
        /// <summary>
        ///     Creates an instance of <see cref="SasContextInitializer"/>
        /// </summary>
        /// <param name="modelBuilder">The db model builder to use</param>
        public SasContextInitializer(DbModelBuilder modelBuilder)
            : base(modelBuilder, true)
        {
        }
    }
}