namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Model;

    /// <summary>
    ///     Configuration for the <see cref="Host" /> entity
    /// </summary>
    public class HostMap : IEntityTypeConfiguration<Host>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HostMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<Host> builder)
        {
            builder.ToTable(nameof(Host));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.HostId)
                .IsRequired();

            builder.Property(t => t.Address)
                .IsRequired();

            builder.Property(t => t.Registered)
                .IsRequired();
        }
    }
}