namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System.Data.Entity;
    using System.Data.SQLite;
    using Monaco.Common.Storage;
    using Protocol.Common.Storage;

    [DbConfigurationType(typeof(SQLiteConfiguration))]
    public class BingoContext : DbContext
    {
        /// <summary>
        ///     Gets a set of <see cref="Certificate"/> items.
        /// </summary>
        public DbSet<Certificate> Certificates { get; set; }

        public BingoContext(IConnectionStringResolver connectionStringResolver)
            : base(new SQLiteConnection(connectionStringResolver.Resolve()), true)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new BingoServerSettingsModelConfiguration());
            modelBuilder.Configurations.Add(new HostConfiguration());
            modelBuilder.Configurations.Add(new ReportTransactionModelConfiguration());
            modelBuilder.Configurations.Add(new ReportEventModelConfiguration());
            modelBuilder.Configurations.Add(new WinResultModelConfiguration());
            modelBuilder.Configurations.Add(new CertificateConfiguration());
            modelBuilder.Configurations.Add(new BingoDaubsModelConfiguration());
            modelBuilder.Configurations.Add(new PendingJackpotAwardsConfiguration());
            Database.SetInitializer(new BingoContextInitializer(modelBuilder));
        }
    }
}
