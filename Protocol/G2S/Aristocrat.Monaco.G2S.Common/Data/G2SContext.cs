namespace Aristocrat.Monaco.G2S.Common.Data.Models
{
    using Aristocrat.Monaco.G2S.Data.Model;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Monaco.Common.Storage;
    using Protocol.Common.Storage;

    /// <summary>
    ///     
    /// </summary>
    public class G2SContext : DbContext
    {
        private readonly string _connectionString;

        /// <summary>
        ///     Initializes a new instance of the <see cref="G2SContext" /> class.
        /// </summary>
        /// <param name="connectionStringResolver">Connection string.</param>
        public G2SContext(IConnectionStringResolver connectionStringResolver)
        {
            _connectionString = connectionStringResolver.Resolve();
        }

        /// <summary>
        ///  Override
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);

        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new PendingJackpotAwardsConfiguration());
        }
    }
}
