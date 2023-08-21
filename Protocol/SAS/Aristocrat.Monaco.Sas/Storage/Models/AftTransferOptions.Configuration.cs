namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     The aft transfer options persistence configuration
    /// </summary>
    public class AftTransferOptionsConfiguration : IEntityTypeConfiguration<AftTransferOptions>
    {
        /// <summary>
        ///     Creates an instance of <see cref="AftTransferOptionsConfiguration"/>
        /// </summary>
        public void Configure(EntityTypeBuilder<AftTransferOptions> builder)
        {
            builder.ToTable(nameof(AftTransferOptions));
            builder.HasKey(x => x.Id);
            builder.Property(x => x.IsTransferAcknowledgedByHost)
                .IsRequired();
            builder.Property(x => x.CurrentTransfer)
                .IsRequired();
            builder.Property(x => x.CurrentTransferFlags)
                .IsRequired();
        }
    }
}