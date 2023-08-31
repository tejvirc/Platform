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
            optionsBuilder.UseSqlite(_connectionString);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BingoServerSettingsModelConfiguration());
            modelBuilder.ApplyConfiguration(new HostConfiguration());
            modelBuilder.ApplyConfiguration(new ReportTransactionModelConfiguration());
            modelBuilder.ApplyConfiguration(new ReportEventModelConfiguration());
            modelBuilder.ApplyConfiguration(new WinResultModelConfiguration());
            modelBuilder.ApplyConfiguration(new CertificateConfiguration());
            modelBuilder.ApplyConfiguration(new BingoDaubsModelConfiguration());
        }
    }
}
