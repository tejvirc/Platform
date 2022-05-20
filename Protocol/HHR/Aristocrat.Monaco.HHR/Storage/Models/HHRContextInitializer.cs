namespace Aristocrat.Monaco.Hhr.Storage.Models
{
    using System.Data.Entity;
    using SQLite.CodeFirst;

    /// <summary>
    ///     
    /// </summary>
    public class HHRContextInitializer : SqliteCreateDatabaseIfNotExists<HHRContext>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HHRContextInitializer"/> class.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public HHRContextInitializer(DbModelBuilder modelBuilder)
            : base(modelBuilder, true)
        {
        }
    }
}
