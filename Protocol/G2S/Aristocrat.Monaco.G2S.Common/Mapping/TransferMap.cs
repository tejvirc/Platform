namespace Aristocrat.Monaco.G2S.Common.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using PackageManager.Storage;

    /// <summary>
    ///     Configuration for the <see cref="TransferEntity" /> entity
    /// </summary>
    public class TransferMap : IEntityTypeConfiguration<TransferEntity>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransferMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<TransferEntity> builder)
        {
            builder.ToTable("Transfer");

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.TransferId)
                .IsRequired();

            builder.Property(t => t.Location)
                .IsRequired();

            builder.Property(t => t.Parameters)
                .IsRequired();

            builder.Property(t => t.ReasonCode)
                .IsRequired();

            builder.Property(t => t.TransferType)
                .IsRequired();

            builder.Property(t => t.State)
                .IsRequired();

            builder.Property(t => t.Exception)
                .IsRequired();

            builder.Property(t => t.DeleteAfter)
                .IsRequired();

            builder.Property(t => t.Size)
                .IsRequired();

            builder.Property(t => t.TransferPaused)
                .IsRequired();

            builder.Property(t => t.TransferSize)
                .IsRequired();

            builder.Property(t => t.TransferCompletedDateTime);
        }
    }
}