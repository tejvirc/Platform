namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     The database configuration for <see cref="ValidationInformation"/>
    /// </summary>
    public class ValidationInformationConfiguration : IEntityTypeConfiguration<ValidationInformation>
    {
        /// <summary>
        ///     Creates an instance of <see cref="ValidationInformationConfiguration"/>
        /// </summary>
        public void Configure(EntityTypeBuilder<ValidationInformation> builder)
        {
            builder.ToTable(nameof(ValidationInformation));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ExtendedTicketDataStatus)
                .IsRequired();
            builder.Property(x => x.ExtendedTicketDataSet)
                .IsRequired();
            builder.Property(x => x.LastReceivedSequenceNumber)
                .IsRequired();
            builder.Property(x => x.SequenceNumber)
                .IsRequired();
            builder.Property(x => x.MachineValidationId)
                .IsRequired();
            builder.Property(x => x.ValidationConfigured)
                .IsRequired();
        }
    }
}