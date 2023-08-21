namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     The configuration for the Aft Registration table
    /// </summary>
    public class AftRegistrationConfiguration : IEntityTypeConfiguration<AftRegistration>
    {
        /// <summary>
        ///     Creates an instance of <see cref="AftRegistrationConfiguration"/>
        /// </summary>
        public void Configure(EntityTypeBuilder<AftRegistration> builder)
        {
            builder.ToTable(nameof(AftRegistration));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RegistrationStatus)
                .IsRequired();
            builder.Property(x => x.PosId)
                .IsRequired();
            builder.Property(x => x.AftRegistrationKey)
                .IsRequired()
                .IsFixedLength();
        }
    }
}