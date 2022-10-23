namespace Aristocrat.Monaco.G2S.Common.CertificateManager.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Models;

    /// <summary>
    ///     Configuration for the <see cref="Certificate" /> entity
    /// </summary>
    public class CertificateMap : IEntityTypeConfiguration<Certificate>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CertificateMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<Certificate> builder)
        {
            builder.ToTable(nameof(Certificate));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Thumbprint)
                .IsRequired();

            builder.Property(t => t.Data)
                .IsRequired();

            builder.Property(t => t.Password)
                .IsRequired();

            builder.Property(t => t.VerificationDate)
                .IsRequired();

            builder.Property(t => t.OcspOfflineDate);

            builder.Property(t => t.Status)
                .IsRequired();

            builder.Property(t => t.Default)
                .IsRequired();
        }
    }
}