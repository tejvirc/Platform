namespace Aristocrat.Monaco.G2S.Common.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using PackageManager.Storage;

    /// <summary>
    ///     Configuration for the <see cref="PackageError" /> entity
    /// </summary>
    public class PackageErrorMap : EntityTypeConfiguration<PackageError>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PackageErrorMap" /> class.
        /// </summary>
        public PackageErrorMap()
        {
            ToTable("PackageError");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.ErrorCode)
                .IsRequired();

            Property(t => t.ErrorMessage)
                .IsRequired();
        }
    }
}