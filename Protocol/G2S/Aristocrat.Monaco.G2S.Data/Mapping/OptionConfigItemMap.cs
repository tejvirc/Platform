namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using OptionConfig;

    /// <summary>
    ///     Configuration for the <see cref="OptionConfigItem" /> entity
    /// </summary>
    public class OptionConfigItemMap : IEntityTypeConfiguration<OptionConfigItem>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionConfigItemMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<OptionConfigItem> builder)
        {
            builder.ToTable(nameof(OptionConfigItem));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.OptionId)
                .IsRequired();

            builder.Property(t => t.SecurityLevel)
                .IsRequired();

            builder.Property(t => t.MinSelections)
                .IsRequired();

            builder.Property(t => t.MaxSelections)
                .IsRequired();

            builder.Property(t => t.Duplicates)
                .IsRequired();

            builder.Property(t => t.Parameters)
                .IsRequired();

            builder.Property(t => t.CurrentValues)
                .IsRequired();

            builder.Property(t => t.DefaultValues)
                .IsRequired();

            builder.Property(t => t.ParameterType)
                .IsRequired();
        }
    }
}