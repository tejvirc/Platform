namespace Aristocrat.Monaco.G2S.Data.Model
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    ///     The database configuration for <see cref="PendingJackpotAwards"/>
    /// </summary>
    public class PendingJackpotAwardsConfiguration : IEntityTypeConfiguration<PendingJackpotAwards>
    {
        /// <summary>
        ///     Configures an instance of <see cref="PendingJackpotAwardsConfiguration"/>
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
