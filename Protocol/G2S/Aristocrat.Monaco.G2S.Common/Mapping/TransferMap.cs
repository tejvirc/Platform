namespace Aristocrat.Monaco.G2S.Common.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using PackageManager.Storage;

    /// <summary>
    ///     Configuration for the <see cref="TransferEntity" /> entity
    /// </summary>
    public class TransferMap : EntityTypeConfiguration<TransferEntity>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransferMap" /> class.
        /// </summary>
        public TransferMap()
        {
            ToTable("Transfer");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.TransferId)
                .IsRequired();

            Property(t => t.Location)
                .IsRequired();

            Property(t => t.Parameters)
                .IsRequired();

            Property(t => t.ReasonCode)
                .IsRequired();

            Property(t => t.TransferType)
                .IsRequired();

            Property(t => t.State)
                .IsRequired();

            Property(t => t.Exception)
                .IsRequired();

            Property(t => t.DeleteAfter)
                .IsRequired();

            Property(t => t.Size)
                .IsRequired();

            Property(t => t.TransferPaused)
                .IsRequired();

            Property(t => t.TransferSize)
                .IsRequired();

            Property(t => t.TransferCompletedDateTime)
                .IsOptional();
        }
    }
}