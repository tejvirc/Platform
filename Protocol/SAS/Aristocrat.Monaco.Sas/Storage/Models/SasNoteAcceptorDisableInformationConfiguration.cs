namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     The database configuration for the <see cref="SasNoteAcceptorDisableInformation"/> entity
    /// </summary>
    public class SasNoteAcceptorDisableInformationConfiguration : IEntityTypeConfiguration<SasNoteAcceptorDisableInformation>
    {
        /// <summary>
        ///     Creates an instance of <see cref="SasNoteAcceptorDisableInformationConfiguration"/>
        /// </summary>
        public void Configure(EntityTypeBuilder<SasNoteAcceptorDisableInformation> builder)
        {
            builder.ToTable(nameof(SasNoteAcceptorDisableInformation));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.DisableReasons)
                .IsRequired();
        }
    }
}
