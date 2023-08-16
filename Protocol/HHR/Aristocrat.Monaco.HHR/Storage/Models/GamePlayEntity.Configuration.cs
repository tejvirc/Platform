namespace Aristocrat.Monaco.Hhr.Storage.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class GamePlayEntityConfiguration : IEntityTypeConfiguration<GamePlayEntity>
    {
        public void Configure(EntityTypeBuilder<GamePlayEntity> builder)
        {
            builder.ToTable(nameof(GamePlayEntity));

            builder.HasKey(t => t.Id);
            builder.Property(t => t.GamePlayRequest).IsRequired();
            builder.Property(t => t.GamePlayResponse).IsRequired();
            builder.Property(t => t.RaceStartRequest).IsRequired();
            builder.Property(t => t.PrizeCalculationError).IsRequired();
        }
    }
}
