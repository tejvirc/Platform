namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using Microsoft.EntityFrameworkCore;
    using System.Reflection;
    using Protocol.Common.Storage;

    /// <summary>
    ///     
    /// </summary>
    public class MgamContext : DbContext
    {
        private readonly string _connectionString;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MgamContext" /> class.
        /// </summary>
        /// <param name="connectionStringResolver">Connection string.</param>
        public MgamContext(IConnectionStringResolver connectionStringResolver)
        {
            _connectionString = connectionStringResolver.Resolve();
            Database.EnsureCreated();
        }

        /// <summary>
        ///     Gets a set of <see cref="Hosts"/> items.
        /// </summary>
        public DbSet<Host> Hosts { get; set; }

        /// <summary>
        ///     Gets a set of <see cref="Devices"/> items.
        /// </summary>
        public DbSet<Device> Devices { get; set; }

        /// <summary>
        ///     Gets a set of <see cref="Applications"/> items.
        /// </summary>
        public DbSet<Application> Applications { get; set; }

        /// <summary>
        ///     Gets a set of <see cref="Installations"/> items.
        /// </summary>
        public DbSet<Installation> Installations { get; set; }

        /// <summary>
        ///     Gets a set of <see cref="Checksums"/> items.
        /// </summary>
        public DbSet<Checksum> Checksums { get; set; }

        /// <summary>
        ///     Gets a set of <see cref="Notifications"/> items.
        /// </summary>
        public DbSet<Notification> Notifications { get; set; }

        /// <summary>
        ///     Gets a set of <see cref="Certificates"/> items.
        /// </summary>
        public DbSet<Certificate> Certificates { get; set; }

        /// <summary>
        ///     Gets a set of <see cref="Session"/> items.
        /// </summary>
        public DbSet<Session> Session { get; set; }

        /// <summary>
        ///     Gets a set of <see cref="Voucher"/> items.
        /// </summary>
        public DbSet<Voucher> Voucher { get; set; }

        /// <summary>
        ///     Gets a set of <see cref="PendingJackpotAwards"/> items.
        /// </summary>
        public DbSet<PendingJackpotAwards> PendingJackpotAwards { get; set; }

        /// <summary>
        ///     Gets a set of <see cref="TransactionRequests"/> items.
        /// </summary>
        public DbSet<TransactionRequests> TransactionRequests { get; set; }

        /// <summary>
        ///     override method
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString, options =>
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });

            base.OnConfiguring(optionsBuilder);
        }

        /// <summary>
        ///     override method
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new HostConfiguration());
            modelBuilder.ApplyConfiguration(new DeviceConfiguration());
            modelBuilder.ApplyConfiguration(new ApplicationConfiguration());
            modelBuilder.ApplyConfiguration(new InstallationConfiguration());
            modelBuilder.ApplyConfiguration(new ChecksumConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationConfiguration());
            modelBuilder.ApplyConfiguration(new CertificateConfiguration());

            modelBuilder.ApplyConfiguration(new SessionConfiguration());
            modelBuilder.ApplyConfiguration(new VoucherConfiguration());
            modelBuilder.ApplyConfiguration(new PendingJackpotAwardsConfiguration());
            modelBuilder.ApplyConfiguration(new TransactionRequestsConfiguration());
        }
    }
}
