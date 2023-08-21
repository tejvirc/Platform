namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     The database configuration for <see cref="PortAssignment"/>
    /// </summary>
    public class PortAssignmentConfiguration : IEntityTypeConfiguration<PortAssignment>
    {
        /// <summary>
        ///     Creates an instance of <see cref="PortAssignmentConfiguration"/>
        /// </summary>
        public void Configure(EntityTypeBuilder<PortAssignment> builder)
        {
            builder.ToTable(nameof(PortAssignment));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.IsDualHost)
                .IsRequired();
            builder.Property(x => x.AftPort)
                .IsRequired();
            builder.Property(x => x.GeneralControlPort)
                .IsRequired();
            builder.Property(x => x.LegacyBonusPort)
                .IsRequired();
            builder.Property(x => x.ProgressivePort)
                .IsRequired();
            builder.Property(x => x.ValidationPort)
                .IsRequired();
            builder.Property(x => x.GameStartEndHosts)
                .IsRequired();
            builder.Property(x => x.Host1NonSasProgressiveHitReporting)
                .IsRequired();
            builder.Property(x => x.Host2NonSasProgressiveHitReporting)
                .IsRequired();
        }
    }
}