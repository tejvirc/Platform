namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     The database configuration for <see cref="EftHistoryItem"/>
    /// </summary>
    public class EftHistoryItemConfiguration : EntityTypeConfiguration<EftHistoryItem>
    {
        /// <summary>
        ///     Creates an instance of <see cref="EftHistoryItemConfiguration"/>
        /// </summary>
        public EftHistoryItemConfiguration()
        {
            ToTable(nameof(EftHistoryItem));
            HasKey(x => x.Id);
            Property(x => x.EftHistoryLog);
        }
    }
}