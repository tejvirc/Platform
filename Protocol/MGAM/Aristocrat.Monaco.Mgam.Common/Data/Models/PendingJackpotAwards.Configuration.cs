namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     Configuration for <see cref="PendingJackpotAwards"/> model.
    /// </summary>
    public class PendingJackpotAwardsConfiguration : IEntityTypeConfiguration<PendingJackpotAwards>
    {
        /// <summary>
        ///     Initializes an instance of the <see cref="PendingJackpotAwardsConfiguration"/> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<PendingJackpotAwards> builder)
        {
            builder.ToTable(nameof(PendingJackpotAwards));

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Awards)
                .IsRequired();
        }
    }
}
