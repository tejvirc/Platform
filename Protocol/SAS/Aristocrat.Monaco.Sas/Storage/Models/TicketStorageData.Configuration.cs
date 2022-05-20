namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     The database configuration for <see cref="TicketStorageData"/> entity
    /// </summary>
    public class TicketStorageDataConfiguration : EntityTypeConfiguration<TicketStorageData>
    {
        /// <summary>
        ///     Creates an instance of <see cref="TicketStorageDataConfiguration"/>
        /// </summary>
        public TicketStorageDataConfiguration()
        {
            ToTable(nameof(TicketStorageData));
            HasKey(x => x.Id);
            Property(x => x.CashableTicketExpiration)
                .IsRequired();
            Property(x => x.VoucherInState)
                .IsRequired();
            Property(x => x.TicketInfoField);
            Property(x => x.PoolId)
                .IsRequired();
            Property(x => x.RedemptionEnabled)
                .IsRequired();
            Property(x => x.TicketExpiration)
                .IsRequired();
            Property(x => x.RestrictedTicketExpiration)
                .IsRequired();
            Property(x => x.RestrictedTicketDefaultExpiration)
                .IsRequired();
            Property(x => x.RestrictedTicketCombinedExpiration)
                .IsRequired();
            Property(x => x.RestrictedTicketIndependentExpiration)
                .IsRequired();
            Property(x => x.RestrictedTicketCreditsExpiration)
                .IsRequired();
        }
    }
}