namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     The database configuration for <see cref="ExceptionQueue"/>
    /// </summary>
    public class ExceptionQueueConfiguration : IEntityTypeConfiguration<ExceptionQueue>
    {
        /// <summary>
        ///     Creates an instance of <see cref="ExceptionQueueConfiguration"/>
        /// </summary>
        public void Configure(EntityTypeBuilder<ExceptionQueue> builder)
        {
            builder.ToTable(nameof(ExceptionQueue));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ClientId);
            builder.Property(x => x.Queue)
                .IsRequired();
            builder.Property(x => x.PriorityQueue)
                .IsRequired();
        }
    }
}