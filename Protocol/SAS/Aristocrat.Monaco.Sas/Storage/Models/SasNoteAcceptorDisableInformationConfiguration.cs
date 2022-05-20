namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     The database configuration for the <see cref="SasNoteAcceptorDisableInformation"/> entity
    /// </summary>
    public class SasNoteAcceptorDisableInformationConfiguration : EntityTypeConfiguration<SasNoteAcceptorDisableInformation>
    {
        /// <summary>
        ///     Creates an instance of <see cref="SasNoteAcceptorDisableInformationConfiguration"/>
        /// </summary>
        public SasNoteAcceptorDisableInformationConfiguration()
        {
            ToTable(nameof(SasNoteAcceptorDisableInformation));
            HasKey(x => x.Id);

            Property(x => x.DisableReasons)
                .IsRequired();
        }
    }
}
