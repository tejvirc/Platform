namespace Aristocrat.Monaco.G2S.Common.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using PackageManager.Storage;

    /// <summary>
    ///     Configuration for the <see cref="PackageError" /> entity
    /// </summary>
    public class PackageErrorMap : IEntityTypeConfiguration<PackageError>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PackageErrorMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<PackageError> builder)
        {
            builder.ToTable(nameof(PackageError));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.ErrorCode).IsRequired();

            builder.Property(t => t.ErrorMessage).IsRequired();
        }
    }
}