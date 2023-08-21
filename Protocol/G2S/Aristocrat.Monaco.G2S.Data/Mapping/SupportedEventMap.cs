namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Model;

    /// <summary>
    ///     Configuration for the <see cref="SupportedEvent" /> entity
    /// </summary>
    public class SupportedEventMap : IEntityTypeConfiguration<SupportedEvent>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SupportedEventMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<SupportedEvent> builder)
        {
            builder.ToTable(nameof(SupportedEvent));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.DeviceId)
                .IsRequired();

            builder.Property(t => t.DeviceClass)
                .IsRequired();

            builder.Property(t => t.EventCode)
                .IsRequired();
        }
    }
}