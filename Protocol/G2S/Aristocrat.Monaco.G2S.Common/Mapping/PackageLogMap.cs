namespace Aristocrat.Monaco.G2S.Common.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Aristocrat.Monaco.G2S.Data.Model;

    /// <summary>
    ///     Configuration for the <see cref="PackageLog" /> entity
    /// </summary>
    public class PackageLogMap : IEntityTypeConfiguration<PackageLog>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PackageLogMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<PackageLog> builder)
        {
            builder.ToTable(nameof(PackageLog));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.PackageId)
                .IsRequired();

            builder.Property(t => t.Size)
                .IsRequired();

            builder.Property(t => t.ReasonCode)
                .IsRequired();

            builder.Property(t => t.ErrorCode);

            builder.Property(t => t.State)
                .IsRequired();

            builder.Property(t => t.Exception);

            builder.Property(t => t.DeviceId)
                .IsRequired();

            builder.Property(t => t.TransactionId)
                .IsRequired();

            builder.Property(t => t.ActivityDateTime);

            builder.Property(t => t.ActivityType);

            builder.Property(t => t.Overwrite);

            builder.Property(t => t.Hash);

            builder.Property(t => t.TransferId);

            builder.Property(t => t.Location);

            builder.Property(t => t.Parameters);

            builder.Property(t => t.TransferState);

            builder.Property(t => t.TransferType);

            builder.Property(t => t.DeleteAfter);

            builder.Property(t => t.TransferPaused);

            builder.Property(t => t.TransferSize);

            builder.Property(t => t.TransferCompletedDateTime);

            builder.Property(t => t.PackageValidateDateTime);
        }
    }
}