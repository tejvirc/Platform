namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     Configuration for <see cref="Voucher"/> model.
    /// </summary>
    public class VoucherConfiguration : EntityTypeConfiguration<Voucher>
    {
        /// <summary>
        ///     Initializes an instance of the <see cref="VoucherConfiguration"/> class.
        /// </summary>
        public VoucherConfiguration()
        {
            ToTable(nameof(Voucher));

            HasKey(t => t.Id);

            Property(t => t.VoucherBarcode)
                .IsRequired();

            Property(t => t.CasinoName)
                .IsRequired();

            Property(t => t.CasinoAddress)
                .IsRequired();

            Property(t => t.VoucherType)
                .IsRequired();

            Property(t => t.CashAmount)
                .IsRequired();

            Property(t => t.CouponAmount)
                .IsRequired();

            Property(t => t.TotalAmount)
                .IsRequired();

            Property(t => t.AmountLongForm)
                .IsRequired();

            Property(t => t.Date)
                .IsRequired();

            Property(t => t.Time)
                .IsRequired();

            Property(t => t.Expiration)
                .IsRequired();

            Property(t => t.DeviceId)
                .IsRequired();

            Property(t => t.OfflineReason)
                .IsRequired();
        }
    }
}
