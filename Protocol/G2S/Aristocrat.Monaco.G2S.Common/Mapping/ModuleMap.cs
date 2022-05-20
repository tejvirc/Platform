namespace Aristocrat.Monaco.G2S.Common.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using PackageManager.Storage;

    /// <summary>
    ///     Configuration for the <see cref="Module" /> entity
    /// </summary>
    public class ModuleMap : EntityTypeConfiguration<Module>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ModuleMap" /> class.
        /// </summary>
        public ModuleMap()
        {
            ToTable("Module");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.ModuleId)
                .IsRequired();

            Property(t => t.Status)
                .IsRequired();
        }
    }
}