namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     The database configuration for the <see cref="SasDisableInformation"/> entity
    /// </summary>
    public class SasDisableInformationConfiguration : EntityTypeConfiguration<SasDisableInformation>
    {
        /// <summary>
        ///     Creates an instance of <see cref="SasDisableInformationConfiguration"/>
        /// </summary>
        public SasDisableInformationConfiguration()
        {
            ToTable(nameof(SasDisableInformation));
            HasKey(x => x.Id);

            Property(x => x.DisableStates)
                .IsRequired();
        }
    }
}