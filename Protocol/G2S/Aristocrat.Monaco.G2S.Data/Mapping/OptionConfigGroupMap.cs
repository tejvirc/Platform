namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using OptionConfig;

    /// <summary>
    ///     Configuration for the <see cref="OptionConfigGroup" /> entity
    /// </summary>
    public class OptionConfigGroupMap : IEntityTypeConfiguration<OptionConfigGroup>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionConfigGroupMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<OptionConfigGroup> builder)
        {
            builder.ToTable(nameof(OptionConfigGroupMap));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.OptionGroupId)
                .IsRequired();

            builder.Property(t => t.OptionGroupName)
                .IsRequired();

            builder.HasMany(x => x.OptionConfigItems)
                .WithOne(x => x.OptionConfigGroup)
                .HasForeignKey(x => x.OptionConfigGroupId);
        }
    }
}