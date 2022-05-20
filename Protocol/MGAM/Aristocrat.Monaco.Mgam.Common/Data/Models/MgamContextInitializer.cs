namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using System.Data.Entity;
    using SQLite.CodeFirst;

    /// <summary>
    ///     
    /// </summary>
    public class MgamContextInitializer : SqliteCreateDatabaseIfNotExists<MgamContext>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MgamContextInitializer"/> class.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public MgamContextInitializer(DbModelBuilder modelBuilder)
            : base(modelBuilder, true)
        {
        }
    }
}
