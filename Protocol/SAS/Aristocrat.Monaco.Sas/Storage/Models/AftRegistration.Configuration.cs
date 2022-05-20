namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     The configuration for the Aft Registration table
    /// </summary>
    public class AftRegistrationConfiguration : EntityTypeConfiguration<AftRegistration>
    {
        /// <summary>
        ///     Creates an instance of <see cref="AftRegistrationConfiguration"/>
        /// </summary>
        public AftRegistrationConfiguration()
        {
            ToTable(nameof(AftRegistration));
            HasKey(x => x.Id);

            Property(x => x.RegistrationStatus)
                .IsRequired();
            Property(x => x.PosId)
                .IsRequired();
            Property(x => x.AftRegistrationKey)
                .IsRequired()
                .IsFixedLength();
        }
    }
}