namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Model;

    /// <summary>
    ///     Configuration for the <see cref="PrintLog" /> entity
    /// </summary>
    public class PrintLogMap : IEntityTypeConfiguration<PrintLog>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrintLogMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<PrintLog> builder)
        {
            builder.ToTable("PrinterLog");

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.TransactionId).IsRequired();

            builder.Property(t => t.PrinterId).IsRequired();

            builder.Property(t => t.PrintDateTime).IsRequired();

            builder.Property(t => t.TemplateIndex).IsRequired();

            builder.Property(t => t.State).IsRequired();

            builder.Property(t => t.Complete).IsRequired();
        }
    }
}