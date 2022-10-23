namespace Aristocrat.Monaco.G2S.Common.CertificateManager.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Models;

    /// <summary>
    ///     Configuration for the <see cref="PkiConfiguration" /> entity
    /// </summary>
    public class PkiConfigurationMap : IEntityTypeConfiguration<PkiConfiguration>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PkiConfigurationMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<PkiConfiguration> builder)
        {
            builder.ToTable(nameof(PkiConfiguration));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.ScepEnabled)
                .IsRequired();

            builder.Property(t => t.CertificateManagerLocation)
                .HasMaxLength(256);

            builder.Property(t => t.ScepCaIdent)
                .HasMaxLength(256);

            builder.Property(t => t.ScepUsername)
                .HasMaxLength(256);

            builder.Property(t => t.KeySize)
                .IsRequired();

            builder.Property(t => t.ScepManualPollingInterval)
                .IsRequired();

            builder.Property(t => t.OcspEnabled)
                .IsRequired();

            builder.Property(t => t.CertificateStatusLocation)
                .HasMaxLength(256);

            builder.Property(t => t.OcspMinimumPeriodForOffline)
                .IsRequired();

            builder.Property(t => t.OcspReAuthenticationPeriod)
                .IsRequired();

            builder.Property(t => t.OcspAcceptPreviouslyGoodCertificatePeriod)
                .IsRequired();

            builder.Property(t => t.OcspNextUpdate);

            builder.Property(t => t.OfflineMethod)
                .IsRequired();

            builder.Property(t => t.NoncesEnabled)
                .IsRequired();

            builder.Property(t => t.ValidateDomain)
                .IsRequired();

            builder.Property(t => t.CommonName)
                .IsRequired();

            builder.Property(t => t.OrganizationUnit)
                .IsRequired();
        }
    }
}