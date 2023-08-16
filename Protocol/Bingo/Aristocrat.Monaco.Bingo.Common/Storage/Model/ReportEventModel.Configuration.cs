namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ReportEventModelConfiguration : IEntityTypeConfiguration<ReportEventModel>
    {
        public void Configure(EntityTypeBuilder<ReportEventModel> builder)
        {
            builder.ToTable(nameof(ReportEventModel));
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Report).IsRequired();
        }
    }
}