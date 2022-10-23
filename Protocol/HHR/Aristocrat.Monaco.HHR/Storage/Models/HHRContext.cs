namespace Aristocrat.Monaco.Hhr.Storage.Models
{
    using System.Reflection;
    using Microsoft.EntityFrameworkCore;
    using Protocol.Common.Storage;

    /// <summary>
    ///     HHRContext class
    /// </summary>
    public class HHRContext : DbContext
    {
        private readonly string _connectionString;
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="HHRContext" /> class.
        /// </summary>
        /// <param name="connectionStringResolver">Connection string.</param>
        public HHRContext(IConnectionStringResolver connectionStringResolver)
        {
            _connectionString = connectionStringResolver.Resolve();
            Database.EnsureCreated();
        }

        public DbSet<ProgressiveUpdateEntity> ProgressiveUpdateEntity { get; set; }
        public DbSet<PrizeInformationEntity> PrizeInformationEntity { get; set; }
        public DbSet<GamePlayEntity> GamePlayEntity { get; set; }
        public DbSet<PendingRequestEntity> PendingRequestEntity { get; set; }
        public DbSet<ManualHandicapEntity> ManualHandicapEntity { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString, options =>
            {               
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });
            base.OnConfiguring(optionsBuilder);
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ProgressiveUpdateEntityConfiguration());
            modelBuilder.ApplyConfiguration(new PrizeInformationEntityConfiguration());
            modelBuilder.ApplyConfiguration(new GamePlayEntityConfiguration());
            modelBuilder.ApplyConfiguration(new FailedRequestEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ManualHandicapEntityConfiguration());
        }
    }
}
