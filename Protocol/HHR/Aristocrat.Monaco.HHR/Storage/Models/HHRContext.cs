namespace Aristocrat.Monaco.Hhr.Storage.Models
{
    using System;
    using System.IO;
    using Microsoft.EntityFrameworkCore;
    using Protocol.Common.Storage;

    /// <summary>
    ///     HHRContext class
    /// </summary>
    public sealed class HHRContext : DbContext
    {
        private readonly string _connectionString;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HHRContext" /> class.
        /// </summary>
        /// <param name="connectionStringResolver">Connection string.</param>
        public HHRContext(IConnectionStringResolver connectionStringResolver)
        {
            if (connectionStringResolver == null)
            {
                throw new ArgumentNullException(nameof(connectionStringResolver));
            }

            _connectionString = connectionStringResolver.Resolve();
        }

        public DbSet<GamePlayEntity> GamePlayEntity { get; set; }

        public DbSet<PendingRequestEntity> PendingRequestEntity { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var sqliteFile = _connectionString.Replace("Data Source=", string.Empty, StringComparison.OrdinalIgnoreCase);
            if (sqliteFile.EndsWith(".sqlite", StringComparison.OrdinalIgnoreCase) && !File.Exists(sqliteFile))
            {
                using var fs = File.Create(sqliteFile);
            }

            optionsBuilder.UseSqlite(_connectionString);
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.ApplyConfiguration(new ProgressiveUpdateEntityConfiguration());
            modelBuilder.ApplyConfiguration(new PrizeInformationEntityConfiguration());
            modelBuilder.ApplyConfiguration(new GamePlayEntityConfiguration());
            modelBuilder.ApplyConfiguration(new FailedRequestEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ManualHandicapEntityConfiguration());
        }
    }
}
