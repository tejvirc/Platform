namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     The database configuration for <see cref="TicketStorageData"/> entity
    /// </summary>
    public class TicketStorageDataConfiguration : IEntityTypeConfiguration<TicketStorageData>
    {
        /// <summary>
        ///     Creates an instance of <see cref="TicketStorageDataConfiguration"/>
        /// </summary>
        public void Configure(EntityTypeBuilder<TicketStorageData> builder)
        {
            builder.ToTable(nameof(TicketStorageData));
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CashableTicketExpiration)
                .IsRequired();
            builder.Property(x => x.VoucherInState)
                .IsRequired();
            builder.Property(x => x.TicketInfoField);
            builder.Property(x => x.PoolId)
                .IsRequired();
            builder.Property(x => x.RedemptionEnabled)
                .IsRequired();
            builder.Property(x => x.TicketExpiration)
                .IsRequired();
            builder.Property(x => x.RestrictedTicketExpiration)
                .IsRequired();
            builder.Property(x => x.RestrictedTicketDefaultExpiration)
                .IsRequired();
            builder.Property(x => x.RestrictedTicketCombinedExpiration)
                .IsRequired();
            builder.Property(x => x.RestrictedTicketIndependentExpiration)
                .IsRequired();
            builder.Property(x => x.RestrictedTicketCreditsExpiration)
                .IsRequired();
        }
    }
}