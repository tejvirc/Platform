namespace Aristocrat.Monaco.Hhr.Storage.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     Model configuration for an Host that is stored on the site-controller.
    /// </summary>
    public class PrizeInformationEntityConfiguration : EntityTypeConfiguration<PrizeInformationEntity>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrizeInformationEntityConfiguration"/> class.
        /// </summary>
        public PrizeInformationEntityConfiguration()
        {
            ToTable(nameof(PrizeInformationEntity));

            HasKey(t => t.Id);
        }
    }
}
