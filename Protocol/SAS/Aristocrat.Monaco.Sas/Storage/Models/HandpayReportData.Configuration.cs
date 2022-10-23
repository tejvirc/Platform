namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     The database configuration for <see cref="HandpayReportData"/>
    /// </summary>
    public class HandpayReportDataConfiguration : IEntityTypeConfiguration<HandpayReportData>
    {
        /// <summary>
        ///     Creates an instance of <see cref="HandpayReportDataConfiguration"/>
        /// </summary>
        public void Configure(EntityTypeBuilder<HandpayReportData> builder)
        {
            builder.ToTable(nameof(HandpayReportData));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ClientId)
                .IsRequired();
            builder.Property(x => x.Queue)
                .IsRequired();
        }
    }
}