namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using Model;

    /// <summary>
    ///     Configuration for the <see cref="PrintLog" /> entity
    /// </summary>
    public class PrintLogMap : EntityTypeConfiguration<PrintLog>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrintLogMap" /> class.
        /// </summary>
        public PrintLogMap()
        {
            ToTable("PrinterLog");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.TransactionId)
                .IsRequired();

            Property(t => t.PrinterId)
                .IsRequired();

            Property(t => t.PrintDateTime)
                .IsRequired();

            Property(t => t.TemplateIndex)
                .IsRequired();

            Property(t => t.State)
                .IsRequired();

            Property(t => t.Complete)
                .IsRequired();
        }
    }
}