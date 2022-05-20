namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     Model configuration for a Certificate that is stored on the site-controller.
    /// </summary>
    public class CertificateConfiguration : EntityTypeConfiguration<Certificate>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CertificateConfiguration"/> class.
        /// </summary>
        public CertificateConfiguration()
        {
            ToTable(nameof(Certificate));

            HasKey(t => t.Id);

            Property(t => t.Thumbprint)
                .IsRequired();

            Property(t => t.RawData)
                .IsRequired();
        }
    }
}
