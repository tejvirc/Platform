namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ReportTransactionModelConfiguration : IEntityTypeConfiguration<ReportTransactionModel>
    {
        public void Configure(EntityTypeBuilder<ReportTransactionModel> builder)
        {
            builder.ToTable(nameof(ReportTransactionModel));
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Report).IsRequired();
        }
    }
}