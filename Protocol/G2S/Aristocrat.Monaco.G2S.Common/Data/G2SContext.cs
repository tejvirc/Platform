namespace Aristocrat.Monaco.G2S.Common.Data.Models
{
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.SQLite;
    using Aristocrat.Monaco.G2S.Data.Model;
    using Monaco.Common.Storage;
    using Protocol.Common.Storage;

    /// <summary>
    ///     
    /// </summary>
    [DbConfigurationType(typeof(SQLiteConfiguration))]
    public class G2SContext : DbContext
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="G2SContext" /> class.
        /// </summary>
        /// <param name="connectionStringResolver">Connection string.</param>
        public G2SContext(IConnectionStringResolver connectionStringResolver)
            : base(new SQLiteConnection(connectionStringResolver.Resolve()), true)
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        /// <inheritdoc />
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions
                .Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<PendingJackpotAwards>().ToTable(nameof(PendingJackpotAwards));
            modelBuilder.Entity<PendingJackpotAwards>().HasKey(t => t.Id);
            modelBuilder.Entity<PendingJackpotAwards>().Property(t => t.Awards).IsRequired();
            //modelBuilder.Configurations.Add(new PendingJackpotAwardsConfiguration());

            Database.SetInitializer(new G2SContextInitializer(modelBuilder));
        }
    }
}
