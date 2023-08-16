namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using CommConfig;
    using System.Diagnostics;
    using System.Reflection.Emit;

    /// <summary>
    ///     Configuration for the <see cref="CommHostConfig" /> entity
    /// </summary>
    public class CommHostConfigMap : IEntityTypeConfiguration<CommHostConfig>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CommHostConfigMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<CommHostConfig> builder)
        {
            builder.ToTable(nameof(CommHostConfig));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.HasMany(x => x.CommHostConfigItems)
                .WithOne()
                .HasForeignKey(item => item.CommHostConfigId);
        }
    }
}