namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System.Data.Entity.ModelConfiguration;

    public class ReportEventModelConfiguration : EntityTypeConfiguration<ReportEventModel>
    {
        public ReportEventModelConfiguration()
        {
            ToTable(nameof(ReportEventModel));
            HasKey(x => x.Id);
            Property(x => x.Report).IsRequired();
        }
    }
}