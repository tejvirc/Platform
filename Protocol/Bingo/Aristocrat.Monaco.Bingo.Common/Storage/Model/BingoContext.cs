namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using Microsoft.EntityFrameworkCore;
    using Protocol.Common.Storage;

    public class BingoContext : DbContext
    {
        private readonly string _connectionString;

        /// <summary>
        ///     Gets a set of <see cref="Certificate"/> items.
        /// </summary>
        public DbSet<Certificate> Certificates { get; set; }

        public BingoContext(IConnectionStringResolver connectionStringResolver)
        {
            _connectionString = connectionStringResolver.Resolve();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
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
