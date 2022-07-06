namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     The <see cref="BingoDaubsModel"/> configuration for persistence
    /// </summary>
    public class BingoDaubsModelConfiguration : EntityTypeConfiguration<BingoDaubsModel>
    {
        /// <summary>
        ///     Creates an instance of <see cref="BingoDaubsModelConfiguration"/>
        /// </summary>
        public BingoDaubsModelConfiguration()
        {
            ToTable(nameof(BingoDaubsModel));
            HasKey(x => x.Id);
            Property(x => x.CardIsDaubed).IsRequired();
        }
    }
}