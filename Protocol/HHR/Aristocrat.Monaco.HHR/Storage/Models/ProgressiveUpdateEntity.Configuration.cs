namespace Aristocrat.Monaco.Hhr.Storage.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     Configuration for the <see cref="ProgressiveUpdateEntity" /> entity
    /// </summary>
    public class ProgressiveUpdateEntityConfiguration : IEntityTypeConfiguration<ProgressiveUpdateEntity>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveUpdateEntityConfiguration"/> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<ProgressiveUpdateEntity> builder)
        {
            builder.ToTable(nameof(ProgressiveUpdateEntity));
            builder.HasKey(t => t.Id);
        }
    }
}
