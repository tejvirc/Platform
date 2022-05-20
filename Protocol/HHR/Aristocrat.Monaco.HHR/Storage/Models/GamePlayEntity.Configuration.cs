namespace Aristocrat.Monaco.Hhr.Storage.Models
{
    using System.Data.Entity.ModelConfiguration;

    public class GamePlayEntityConfiguration : EntityTypeConfiguration<GamePlayEntity>
    {
	    public GamePlayEntityConfiguration()
	    {
		    ToTable(nameof(GamePlayEntity));

		    HasKey(t => t.Id);
            Property(t => t.GamePlayRequest)
                .IsRequired();
            Property(t => t.GamePlayResponse)
                .IsRequired();
            Property(t => t.RaceStartRequest)
                .IsRequired();
            Property(t => t.PrizeCalculationError)
                .IsRequired();
        }
    }
}
