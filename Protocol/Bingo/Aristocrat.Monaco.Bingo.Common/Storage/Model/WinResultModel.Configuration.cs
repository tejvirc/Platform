namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class WinResultModelConfiguration : IEntityTypeConfiguration<WinResultModel>
    {
        public void Configure(EntityTypeBuilder<WinResultModel> builder)
        {
            builder.ToTable(nameof(WinResultModel));
            builder.HasKey(x => x.Id);
            builder.Property(x => x.IsTotalWinMismatched).IsRequired();
        }
    }
}
