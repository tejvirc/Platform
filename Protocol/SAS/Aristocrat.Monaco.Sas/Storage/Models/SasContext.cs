namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Protocol.Common.Storage;

    /// <summary>
    ///     The Sas database context
    /// </summary>
    public class SasContext : DbContext
    {
        private readonly string _connectionString;

        /// <summary>
        ///     Creates an instance of <see cref="SasContext"/>
        /// </summary>
        /// <param name="connectionStringResolver">An instance of <see cref="IConnectionStringResolver"/></param>
        public SasContext(IConnectionStringResolver connectionStringResolver)
        {
            if (connectionStringResolver == null)
            {
                throw new ArgumentNullException(nameof(connectionStringResolver));
            }

            _connectionString = connectionStringResolver.Resolve();
        }

        /// <summary>
        ///     Gets a set of AftHistoryItems.
        /// </summary>
        public DbSet<AftHistoryItem> AftHistoryItem { get; set; }

        /// <summary>
        ///     Gets a set of AftRegistration.
        /// </summary>
        public DbSet<AftRegistration> AftRegistration { get; set; }

        /// <summary>
        ///     Gets a set of AftRegistration.
        /// </summary>
        public DbSet<AftTransferOptions> AftTransferOptions { get; set; }
        /// <summary>
        ///     Gets a set of EnhancedValidationItem.
        /// </summary>
        public DbSet<EnhancedValidationItem> EnhancedValidationItem { get; set; }
        /// <summary>
        ///     Gets a set of ExceptionQueue.
        /// </summary>
        public DbSet<ExceptionQueue> ExceptionQueue { get; set; }

        /// <summary>
        ///     Gets a set of HandpayReportData.
        /// </summary>
        public DbSet<HandpayReportData> HandpayReportData { get; set; }
        /// <summary>
        ///     Gets a set of Host.
        /// </summary>
        public DbSet<Host> Host { get; set; }

        /// <summary>
        ///     Gets a set of PortAssignment.
        /// </summary>
        public DbSet<PortAssignment> PortAssignment { get; set; }

        /// <summary>
        ///     Gets a set of SasDisableInformation.
        /// </summary>
        public DbSet<SasDisableInformation> SasDisableInformation { get; set; }

        /// <summary>
        ///     Gets a set of SasFeatures.
        /// </summary>
        public DbSet<Contracts.SASProperties.SasFeatures> SasFeatures { get; set; }
        /// <summary>
        ///     Gets a set of SasNoteAcceptorDisableInformation.
        /// </summary>
        public DbSet<SasNoteAcceptorDisableInformation> SasNoteAcceptorDisableInformation { get; set; }
        /// <summary>
        ///     Gets a set of TicketStorageData.
        /// </summary>
        public DbSet<TicketStorageData> TicketStorageData { get; set; }
        /// <summary>
        ///     Gets a set of ValidationInformation.
        /// </summary>
        public DbSet<ValidationInformation> ValidationInformation { get; set; }

        /// <summary>
        ///     override method
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);
        }

        /// <summary>
        ///     override method
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.ApplyConfiguration(new AftHistoryItemConfiguration());
            modelBuilder.ApplyConfiguration(new AftRegistrationConfiguration());
            modelBuilder.ApplyConfiguration(new AftTransferOptionsConfiguration());
            modelBuilder.ApplyConfiguration(new EnhancedValidationItemConfiguration());
            modelBuilder.ApplyConfiguration(new ExceptionQueueConfiguration());
            modelBuilder.ApplyConfiguration(new HandpayReportDataConfiguration());
            modelBuilder.ApplyConfiguration(new HostConfiguration());
            modelBuilder.ApplyConfiguration(new PortAssignmentConfiguration());
            modelBuilder.ApplyConfiguration(new SasDisableInformationConfiguration());
            modelBuilder.ApplyConfiguration(new SasFeaturesConfiguration());
            modelBuilder.ApplyConfiguration(new SasNoteAcceptorDisableInformationConfiguration());
            modelBuilder.ApplyConfiguration(new TicketStorageDataConfiguration());
            modelBuilder.ApplyConfiguration(new ValidationInformationConfiguration());
        }
    }
}
