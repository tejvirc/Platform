namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System.Data.Entity.ModelConfiguration;

    public class WinResultModelConfiguration : EntityTypeConfiguration<WinResultModel>
    {
        public WinResultModelConfiguration()
        {
            ToTable(nameof(WinResultModel));
            HasKey(x => x.Id);
            Property(x => x.IsTotalWinMismatched).IsRequired();
        }
    }
}
