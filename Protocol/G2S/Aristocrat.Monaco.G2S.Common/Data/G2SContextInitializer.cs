namespace Aristocrat.Monaco.G2S.Common.Data.Models
{
    using System.Data.Entity;
    using SQLite.CodeFirst;

    /// <summary>
    ///     
    /// </summary>
    public class G2SContextInitializer : SqliteCreateDatabaseIfNotExists<G2SContext>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="G2SContextInitializer"/> class.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public G2SContextInitializer(DbModelBuilder modelBuilder)
            : base(modelBuilder, true)
        {
        }
    }
}
