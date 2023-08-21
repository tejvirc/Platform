namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     The database configuration for <see cref="AftHistoryItem"/>
    /// </summary>
    public class AftHistoryItemConfiguration : IEntityTypeConfiguration<AftHistoryItem>
    {
        /// <summary>
        ///     Creates an instance of <see cref="AftHistoryItemConfiguration"/>
        /// </summary>
        public void Configure(EntityTypeBuilder<AftHistoryItem> builder)
        {
            builder.ToTable(nameof(AftHistoryItem));
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CurrentBufferIndex).IsRequired();
            builder.Property(x => x.AftHistoryLog);
        }
    }
}