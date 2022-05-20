namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System.Data.Entity;
    using System.Data.SQLite;
    using Common.Storage;
    using Protocol.Common.Storage;

    /// <summary>
    ///     The Sas database context
    /// </summary>
    [DbConfigurationType(typeof(SQLiteConfiguration))]
    public class SasContext : DbContext
    {
        /// <summary>
        ///     Creates an instance of <see cref="SasContext"/>
        /// </summary>
        /// <param name="connectionStringResolver">An instance of <see cref="IConnectionStringResolver"/></param>
        public SasContext(
            IConnectionStringResolver connectionStringResolver)
            : base(new SQLiteConnection(connectionStringResolver.Resolve()), true)
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        /// <inheritdoc />
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new AftHistoryItemConfiguration());
            modelBuilder.Configurations.Add(new ValidationInformationConfiguration());
            modelBuilder.Configurations.Add(new TicketStorageDataConfiguration());
            modelBuilder.Configurations.Add(new AftTransferOptionsConfiguration());
            modelBuilder.Configurations.Add(new HostConfiguration());
            modelBuilder.Configurations.Add(new PortAssignmentConfiguration());
            modelBuilder.Configurations.Add(new ExceptionQueueConfiguration());
            modelBuilder.Configurations.Add(new HandpayReportDataConfiguration());
            modelBuilder.Configurations.Add(new SasDisableInformationConfiguration());
            modelBuilder.Configurations.Add(new SasNoteAcceptorDisableInformationConfiguration());
            modelBuilder.Configurations.Add(new SasFeaturesConfiguration());
            modelBuilder.Configurations.Add(new AftRegistrationConfiguration());
            modelBuilder.Configurations.Add(new EnhancedValidationItemConfiguration());
            Database.SetInitializer(new SasContextInitializer(modelBuilder));
        }
    }
}
