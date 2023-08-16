namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     The database configuration for the <see cref="SasDisableInformation"/> entity
    /// </summary>
    public class SasDisableInformationConfiguration : IEntityTypeConfiguration<SasDisableInformation>
    {
        /// <summary>
        ///     Creates an instance of <see cref="SasDisableInformationConfiguration"/>
        /// </summary>
        public void Configure(EntityTypeBuilder<SasDisableInformation> builder)
        {
            builder.ToTable(nameof(SasDisableInformation));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.DisableStates)
                .IsRequired();
        }
    }
}