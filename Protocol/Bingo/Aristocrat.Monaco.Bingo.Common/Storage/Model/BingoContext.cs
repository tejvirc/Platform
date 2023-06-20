namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using Microsoft.EntityFrameworkCore;
    using Monaco.Common.Storage;
    using Protocol.Common.Storage;
    using System;
    using System.IO;

    public class BingoContext : DbContext
    {
        private readonly string _connectionString;

        public BingoContext(IConnectionStringResolver connectionStringResolver)
        {
            _connectionString = connectionStringResolver.Resolve();
        }

        /// <summary>
        ///     Gets a set of <see cref="BingoServerSettingsModel"/> items.
        /// </summary>
        public DbSet<BingoServerSettingsModel> BingoServerSettingsModel { get; set; }
        public DbSet<Host> Host { get; set; }
        public DbSet<ReportTransactionModel> ReportTransactionModel { get; set; }
        public DbSet<ReportEventModel> ReportEventModel { get; set; }
        public DbSet<WinResultModel> WinResultModel { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<BingoDaubsModel> BingoDaubsModel { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var sqliteFile = _connectionString.Replace("Data Source=", string.Empty, StringComparison.OrdinalIgnoreCase);
            if (sqliteFile.EndsWith(".sqlite") && !File.Exists(sqliteFile))
            {
                using (var fs = File.Create(sqliteFile)) { }
            }

            base.OnConfiguring(optionsBuilder);
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
