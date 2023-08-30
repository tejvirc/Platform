namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     Configuration for <see cref="PendingJackpotAwards"/> model.
    /// </summary>
    public class PendingJackpotAwardsConfiguration : EntityTypeConfiguration<PendingJackpotAwards>
    {
        /// <summary>
        ///     Initializes an instance of the <see cref="PendingJackpotAwardsConfiguration"/> class.
        /// </summary>
        public PendingJackpotAwardsConfiguration()
        {
            ToTable(nameof(PendingJackpotAwards));

            HasKey(t => t.Id);

            Property(t => t.Awards)
                .IsRequired();
        }
    }
}
