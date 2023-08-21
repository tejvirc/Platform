namespace Aristocrat.Monaco.G2S.Common.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using PackageManager.Storage;

    /// <summary>
    ///     Configuration for the <see cref="Package" /> entity
    /// </summary>
    public class PackageMap : IEntityTypeConfiguration<Package>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PackageMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<Package> builder)
        {
            builder.ToTable(nameof(Package));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.PackageId)
                .IsRequired();

            builder.Property(t => t.Size)
                .IsRequired();

            builder.Property(t => t.Hash);
        }
    }
}