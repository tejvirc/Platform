namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     Model configuration for a Certificate that is stored on the site-controller.
    /// </summary>
    public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CertificateConfiguration"/> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<Certificate> builder)
        {
            builder.ToTable(nameof(Certificate));

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Thumbprint)
                .IsRequired();

            builder.Property(t => t.RawData)
                .IsRequired();
        }
    }
}
