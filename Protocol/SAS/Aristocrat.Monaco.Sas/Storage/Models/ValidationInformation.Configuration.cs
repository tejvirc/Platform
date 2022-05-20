namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     The database configuration for <see cref="ValidationInformation"/>
    /// </summary>
    public class ValidationInformationConfiguration : EntityTypeConfiguration<ValidationInformation>
    {
        /// <summary>
        ///     Creates an instance of <see cref="ValidationInformationConfiguration"/>
        /// </summary>
        public ValidationInformationConfiguration()
        {
            ToTable(nameof(ValidationInformation));
            HasKey(x => x.Id);

            Property(x => x.ExtendedTicketDataStatus)
                .IsRequired();
            Property(x => x.ExtendedTicketDataSet)
                .IsRequired();
            Property(x => x.LastReceivedSequenceNumber)
                .IsRequired();
            Property(x => x.SequenceNumber)
                .IsRequired();
            Property(x => x.MachineValidationId)
                .IsRequired();
            Property(x => x.ValidationConfigured)
                .IsRequired();
        }
    }
}