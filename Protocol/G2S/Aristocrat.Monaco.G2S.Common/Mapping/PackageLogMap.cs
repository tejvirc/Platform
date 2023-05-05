namespace Aristocrat.Monaco.G2S.Common.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using G2S.Data.Model;

    /// <summary>
    ///     Configuration for the <see cref="PackageLog" /> entity
    /// </summary>
    public class PackageLogMap : EntityTypeConfiguration<PackageLog>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PackageLogMap" /> class.
        /// </summary>
        public PackageLogMap()
        {
            ToTable("PackageLog");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.PackageId)
                .IsRequired();

            Property(t => t.Size)
                .IsRequired();

            Property(t => t.ReasonCode)
                .IsRequired();

            Property(t => t.ErrorCode)
                .IsOptional();

            Property(t => t.State)
                .IsRequired();

            Property(t => t.Exception)
                .IsOptional();

            Property(t => t.DeviceId)
                .IsRequired();

            Property(t => t.TransactionId)
                .IsRequired();

            Property(t => t.ActivityDateTime)
                .IsOptional();

            Property(t => t.ActivityType)
                .IsOptional();

            Property(t => t.Overwrite)
                .IsOptional();

            Property(t => t.Hash)
                .IsOptional();

            Property(t => t.TransferId)
                .IsOptional();

            Property(t => t.Location)
                .IsOptional();

            Property(t => t.Parameters)
                .IsOptional();

            Property(t => t.TransferState)
                .IsOptional();

            Property(t => t.TransferType)
                .IsOptional();

            Property(t => t.DeleteAfter)
                .IsOptional();

            Property(t => t.TransferPaused)
                .IsOptional();

            Property(t => t.TransferSize)
                .IsOptional();

            Property(t => t.TransferCompletedDateTime)
                .IsOptional();

            Property(t => t.PackageValidateDateTime)
                .IsOptional();
        }
    }
}