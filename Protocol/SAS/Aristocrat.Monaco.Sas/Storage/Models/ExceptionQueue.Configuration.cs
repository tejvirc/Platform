namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     The database configuration for <see cref="ExceptionQueue"/>
    /// </summary>
    public class ExceptionQueueConfiguration : EntityTypeConfiguration<ExceptionQueue>
    {
        /// <summary>
        ///     Creates an instance of <see cref="ExceptionQueueConfiguration"/>
        /// </summary>
        public ExceptionQueueConfiguration()
        {
            ToTable(nameof(ExceptionQueue));
            HasKey(x => x.Id);

            Property(x => x.ClientId);
            Property(x => x.Queue)
                .IsRequired();
            Property(x => x.PriorityQueue)
                .IsRequired();
        }
    }
}