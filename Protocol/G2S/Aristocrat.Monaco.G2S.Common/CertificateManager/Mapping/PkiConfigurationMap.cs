namespace Aristocrat.Monaco.G2S.Common.CertificateManager.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using Models;

    /// <summary>
    ///     Configuration for the <see cref="PkiConfiguration" /> entity
    /// </summary>
    public class PkiConfigurationMap : EntityTypeConfiguration<PkiConfiguration>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PkiConfigurationMap" /> class.
        /// </summary>
        public PkiConfigurationMap()
        {
            ToTable("PkiConfiguration");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.ScepEnabled)
                .IsRequired();

            Property(t => t.CertificateManagerLocation)
                .HasMaxLength(256);

            Property(t => t.ScepCaIdent)
                .HasMaxLength(256)
                .IsOptional();

            Property(t => t.ScepUsername)
                .HasMaxLength(256);

            Property(t => t.KeySize)
                .IsRequired();

            Property(t => t.ScepManualPollingInterval)
                .IsRequired();

            Property(t => t.OcspEnabled)
                .IsRequired();

            Property(t => t.CertificateStatusLocation)
                .HasMaxLength(256);

            Property(t => t.OcspMinimumPeriodForOffline)
                .IsRequired();

            Property(t => t.OcspReAuthenticationPeriod)
                .IsRequired();

            Property(t => t.OcspAcceptPreviouslyGoodCertificatePeriod)
                .IsRequired();

            Property(t => t.OcspNextUpdate)
                .IsOptional();

            Property(t => t.OfflineMethod)
                .IsRequired();

            Property(t => t.NoncesEnabled)
                .IsRequired();

            Property(t => t.ValidateDomain)
                .IsRequired();

            Property(t => t.CommonName)
                .IsRequired();

            Property(t => t.OrganizationUnit)
                .IsRequired();
        }
    }
}