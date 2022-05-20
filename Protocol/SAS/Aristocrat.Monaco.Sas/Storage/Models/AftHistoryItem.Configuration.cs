namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     The database configuration for <see cref="AftHistoryItem"/>
    /// </summary>
    public class AftHistoryItemConfiguration : EntityTypeConfiguration<AftHistoryItem>
    {
        /// <summary>
        ///     Creates an instance of <see cref="AftHistoryItemConfiguration"/>
        /// </summary>
        public AftHistoryItemConfiguration()
        {
            ToTable(nameof(AftHistoryItem));
            HasKey(x => x.Id);
            Property(x => x.CurrentBufferIndex)
                .IsRequired();
            Property(x => x.AftHistoryLog);
        }
    }
}