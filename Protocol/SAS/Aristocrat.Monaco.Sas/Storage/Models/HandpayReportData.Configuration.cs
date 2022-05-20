namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     The database configuration for <see cref="HandpayReportData"/>
    /// </summary>
    public class HandpayReportDataConfiguration : EntityTypeConfiguration<HandpayReportData>
    {
        /// <summary>
        ///     Creates an instance of <see cref="HandpayReportDataConfiguration"/>
        /// </summary>
        public HandpayReportDataConfiguration()
        {
            ToTable(nameof(HandpayReportData));
            HasKey(x => x.Id);

            Property(x => x.ClientId)
                .IsRequired();
            Property(x => x.Queue)
                .IsRequired();
        }
    }
}