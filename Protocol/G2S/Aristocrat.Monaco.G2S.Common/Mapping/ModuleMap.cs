namespace Aristocrat.Monaco.G2S.Common.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using PackageManager.Storage;

    /// <summary>
    ///     Configuration for the <see cref="Module" /> entity
    /// </summary>
    public class ModuleMap : IEntityTypeConfiguration<Module>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ModuleMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<Module> builder)
        {
            builder.ToTable(nameof(Module));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.ModuleId).IsRequired();

            builder.Property(t => t.Status).IsRequired();
        }
    }
}