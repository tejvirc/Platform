namespace Aristocrat.Monaco.G2S.Common.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using PackageManager.Storage;

    /// <summary>
    ///     Configuration for the <see cref="Package" /> entity
    /// </summary>
    public class PackageMap : EntityTypeConfiguration<Package>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PackageMap" /> class.
        /// </summary>
        public PackageMap()
        {
            ToTable("Package");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.PackageId)
                .IsRequired();

            Property(t => t.Size)
                .IsRequired();

            Property(t => t.Hash)
                .IsOptional();
        }
    }
}