namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System.Data.Entity.ModelConfiguration;

    public class ReportTransactionModelConfiguration : EntityTypeConfiguration<ReportTransactionModel>
    {
        public ReportTransactionModelConfiguration()
        {
            ToTable(nameof(ReportTransactionModel));
            HasKey(x => x.Id);
            Property(x => x.Report).IsRequired();
        }
    }
}