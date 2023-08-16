namespace Aristocrat.Monaco.Hhr.Storage.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     Model configuration for an Host that is stored on the site-controller.
    /// </summary>
    public class PrizeInformationEntityConfiguration : IEntityTypeConfiguration<PrizeInformationEntity>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrizeInformationEntityConfiguration"/> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<PrizeInformationEntity> builder)
        {
            builder.ToTable(nameof(PrizeInformationEntity));

            builder.HasKey(t => t.Id);
        }
    }
}
