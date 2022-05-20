namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using OptionConfig;

    /// <summary>
    ///     Configuration for the <see cref="OptionConfigGroup" /> entity
    /// </summary>
    public class OptionConfigGroupMap : EntityTypeConfiguration<OptionConfigGroup>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionConfigGroupMap" /> class.
        /// </summary>
        public OptionConfigGroupMap()
        {
            ToTable("OptionConfigGroup");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.OptionGroupId)
                .IsRequired();

            Property(t => t.OptionGroupName)
                .IsRequired();

            HasMany(x => x.OptionConfigItems)
                .WithRequired(x => x.OptionConfigGroup)
                .HasForeignKey(x => x.OptionConfigGroupId);
        }
    }
}