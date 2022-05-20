namespace Aristocrat.Monaco.G2S.Common.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using GAT.Storage;

    /// <summary>
    ///     Configuration for the <see cref="GatVerificationRequest" /> entity
    /// </summary>
    public class GatVerificationRequestMap : EntityTypeConfiguration<GatVerificationRequest>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GatVerificationRequestMap" /> class.
        /// </summary>
        public GatVerificationRequestMap()
        {
            ToTable("GatVerificationRequest");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.VerificationId)
                .IsRequired();

            Property(t => t.DeviceId)
                .IsRequired();

            Property(t => t.TransactionId)
                .IsRequired();

            Property(t => t.Completed)
                .IsRequired();

            Property(t => t.DeviceId)
                .IsRequired();

            Property(t => t.FunctionType)
                .IsRequired();

            Property(t => t.EmployeeId)
                .IsRequired();

            Property(t => t.Date)
                .IsRequired();

            HasMany(l => l.ComponentVerifications)
                .WithOptional()
                .HasForeignKey(item => item.RequestId);
        }
    }
}