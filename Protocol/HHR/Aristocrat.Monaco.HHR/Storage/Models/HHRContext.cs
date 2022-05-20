namespace Aristocrat.Monaco.Hhr.Storage.Models
{
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.SQLite;
    using Common.Storage;
    using Protocol.Common.Storage;

    /// <summary>
    ///     
    /// </summary>
    [DbConfigurationType(typeof(SQLiteConfiguration))]
    public class HHRContext : DbContext
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HHRContext" /> class.
        /// </summary>
        /// <param name="connectionStringResolver">Connection string.</param>
        public HHRContext(IConnectionStringResolver connectionStringResolver)
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

            modelBuilder.Configurations.Add(new ProgressiveUpdateEntityConfiguration());
            modelBuilder.Configurations.Add(new PrizeInformationEntityConfiguration());
            modelBuilder.Configurations.Add(new GamePlayEntityConfiguration());
            modelBuilder.Configurations.Add(new FailedRequestEntityConfiguration());
            modelBuilder.Configurations.Add(new ManualHandicapEntityConfiguration());

            Database.SetInitializer(new HHRContextInitializer(modelBuilder));
        }
    }
}
