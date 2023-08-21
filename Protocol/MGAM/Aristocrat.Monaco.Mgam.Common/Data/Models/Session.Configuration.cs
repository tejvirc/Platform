namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     Configuration for <see cref="Session"/> model.
    /// </summary>
    public class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        /// <summary>
        ///     Initializes an instance of the <see cref="SessionConfiguration"/> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.ToTable(nameof(Session));

            builder.HasKey(t => t.Id);

            builder.Property(t => t.SessionId)
                .IsRequired();

            builder.Property(t => t.OfflineVoucherBarcode)
                .IsRequired();

            builder.Property(t => t.OfflineVoucherPrinted)
                .IsRequired();
        }
    }
}
