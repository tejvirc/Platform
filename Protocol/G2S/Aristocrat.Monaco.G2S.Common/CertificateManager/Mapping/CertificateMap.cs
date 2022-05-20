namespace Aristocrat.Monaco.G2S.Common.CertificateManager.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using Models;

    /// <summary>
    ///     Configuration for the <see cref="Certificate" /> entity
    /// </summary>
    public class CertificateMap : EntityTypeConfiguration<Certificate>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CertificateMap" /> class.
        /// </summary>
        public CertificateMap()
        {
            ToTable("Certificate");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.Thumbprint)
                .IsRequired();

            Property(t => t.Data)
                .IsRequired();

            Property(t => t.Password)
                .IsRequired();

            Property(t => t.VerificationDate)
                .IsRequired();

            Property(t => t.OcspOfflineDate)
                .IsOptional();

            Property(t => t.Status)
                .IsRequired();

            Property(t => t.Default)
                .IsRequired();
        }
    }
}