namespace Aristocrat.Monaco.G2S.Common.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using GAT.Storage;

    /// <summary>
    ///     Configuration for the <see cref="GatVerificationRequest" /> entity
    /// </summary>
    public class GatVerificationRequestMap : IEntityTypeConfiguration<GatVerificationRequest>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GatVerificationRequestMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<GatVerificationRequest> builder)
        {
            builder.ToTable(nameof(GatVerificationRequest));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.VerificationId)
                .IsRequired();

            builder.Property(t => t.DeviceId)
                .IsRequired();

            builder.Property(t => t.TransactionId)
                .IsRequired();

            builder.Property(t => t.Completed)
                .IsRequired();

            builder.Property(t => t.DeviceId)
                .IsRequired();

            builder.Property(t => t.FunctionType)
                .IsRequired();

            builder.Property(t => t.EmployeeId)
                .IsRequired();

            builder.Property(t => t.Date)
                .IsRequired();

            builder.HasMany(l => l.ComponentVerifications)
                .WithOne()
                .HasForeignKey(item => item.RequestId);
        }
    }
}