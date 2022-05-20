namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System.Data.Entity;
    using SQLite.CodeFirst;

    public class BingoContextInitializer : SqliteCreateDatabaseIfNotExists<BingoContext>
    {
        public BingoContextInitializer(DbModelBuilder modelBuilder)
            : base(modelBuilder, true)
        {
        }
    }
}