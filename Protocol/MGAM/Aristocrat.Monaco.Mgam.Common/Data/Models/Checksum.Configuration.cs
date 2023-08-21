namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     Configuration for <see cref="Checksum"/> model.
    /// </summary>
    public class ChecksumConfiguration : IEntityTypeConfiguration<Checksum>
    {
        /// <summary>
        ///     Initializes an instance of the <see cref="ChecksumConfiguration"/> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<Checksum> builder)
        {
            builder.ToTable(nameof(Checksum));

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Seed)
                .IsRequired();
        }
    }
}
