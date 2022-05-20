namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using OptionConfig;

    /// <summary>
    ///     Configuration for the <see cref="OptionConfigItem" /> entity
    /// </summary>
    public class OptionConfigItemMap : EntityTypeConfiguration<OptionConfigItem>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionConfigItemMap" /> class.
        /// </summary>
        public OptionConfigItemMap()
        {
            ToTable("OptionConfigItem");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.OptionId)
                .IsRequired();

            Property(t => t.SecurityLevel)
                .IsRequired();

            Property(t => t.MinSelections)
                .IsRequired();

            Property(t => t.MaxSelections)
                .IsRequired();

            Property(t => t.Duplicates)
                .IsRequired();

            Property(t => t.Parameters)
                .IsRequired();

            Property(t => t.CurrentValues)
                .IsRequired();

            Property(t => t.DefaultValues)
                .IsRequired();

            Property(t => t.ParameterType)
                .IsRequired();
        }
    }
}