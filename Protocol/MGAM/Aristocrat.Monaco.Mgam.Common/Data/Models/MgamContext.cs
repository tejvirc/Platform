namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.SQLite;
    using Monaco.Common.Storage;
    using Protocol.Common.Storage;

    /// <summary>
    ///     
    /// </summary>
    [DbConfigurationType(typeof(SQLiteConfiguration))]
    public class MgamContext : DbContext
    {
        /// <summary>
        ///     Gets a set of <see cref="Host"/> items.
        /// </summary>
        public DbSet<Host> Hosts { get; set; }

        /// <summary>
        ///     Gets a set of <see cref="Device"/> items.
        /// </summary>
        public DbSet<Device> Devices { get; set; }

        /// <summary>
        ///     Gets a set of <see cref="Application"/> items.
        /// </summary>
        public DbSet<Application> Applications { get; set; }

        /// <summary>
        ///     Gets a set of <see cref="Installation"/> items.
        /// </summary>
        public DbSet<Installation> Installations { get; set; }

        /// <summary>
        ///     Gets a set of <see cref="Checksum"/> items.
        /// </summary>
        public DbSet<Checksum> Checksums { get; set; }

        /// <summary>
        ///     Gets a set of <see cref="Notification"/> items.
        /// </summary>
        public DbSet<Notification> Notifications { get; set; }

        /// <summary>
        ///     Gets a set of <see cref="Certificate"/> items.
        /// </summary>
        public DbSet<Certificate> Certificates { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MgamContext" /> class.
        /// </summary>
        /// <param name="connectionStringResolver">Connection string.</param>
        public MgamContext(IConnectionStringResolver connectionStringResolver)
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

            modelBuilder.Configurations.Add(new HostConfiguration());

            modelBuilder.Configurations.Add(new CertificateConfiguration());

            modelBuilder.Configurations.Add(new DeviceConfiguration());

            modelBuilder.Configurations.Add(new ApplicationConfiguration());

            modelBuilder.Configurations.Add(new InstallationConfiguration());

            modelBuilder.Configurations.Add(new SessionConfiguration());

            modelBuilder.Configurations.Add(new VoucherConfiguration());

            modelBuilder.Configurations.Add(new ChecksumConfiguration());

            modelBuilder.Configurations.Add(new NotificationConfiguration());

            modelBuilder.Configurations.Add(new PendingJackpotAwardsConfiguration());

            modelBuilder.Configurations.Add(new TransactionRequestsConfiguration());

            Database.SetInitializer(new MgamContextInitializer(modelBuilder));
        }
    }
}
