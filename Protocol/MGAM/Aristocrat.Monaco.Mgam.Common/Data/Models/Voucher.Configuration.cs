namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     Configuration for <see cref="Voucher"/> model.
    /// </summary>
    public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
    {
        /// <summary>
        ///     Initializes an instance of the <see cref="VoucherConfiguration"/> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<Voucher> builder)
        {
            builder.ToTable(nameof(Voucher));

            builder.HasKey(t => t.Id);

            builder.Property(t => t.VoucherBarcode)
                .IsRequired();

            builder.Property(t => t.CasinoName)
                .IsRequired();

            builder.Property(t => t.CasinoAddress)
                .IsRequired();

            builder.Property(t => t.VoucherType)
                .IsRequired();

            builder.Property(t => t.CashAmount)
                .IsRequired();

            builder.Property(t => t.CouponAmount)
                .IsRequired();

            builder.Property(t => t.TotalAmount)
                .IsRequired();

            builder.Property(t => t.AmountLongForm)
                .IsRequired();

            builder.Property(t => t.Date)
                .IsRequired();

            builder.Property(t => t.Time)
                .IsRequired();

            builder.Property(t => t.Expiration)
                .IsRequired();

            builder.Property(t => t.DeviceId)
                .IsRequired();

            builder.Property(t => t.OfflineReason)
                .IsRequired();
        }
    }
}
