namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Model;

    /// <summary>
    ///     Configuration for the <see cref="EventSubscription" /> entity
    /// </summary>
    public class EventSubscriptionMap : IEntityTypeConfiguration<EventSubscription>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EventSubscriptionMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<EventSubscription> builder)
        {
            builder.ToTable(nameof(EventSubscription));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.HostId)
                .IsRequired();

            builder.Property(t => t.DeviceId)
                .IsRequired();

            builder.Property(t => t.EventCode)
                .IsRequired();

            builder.Property(t => t.SubType)
                .IsRequired();

            builder.Property(t => t.DeviceClass)
                .IsRequired();

            builder.Property(t => t.EventPersist)
                .IsRequired();

            builder.Property(t => t.SendClassMeters)
                .IsRequired();

            builder.Property(t => t.SendDeviceMeters)
                .IsRequired();

            builder.Property(t => t.SendDeviceStatus)
                .IsRequired();


            builder.Property(t => t.SendTransaction)
                .IsRequired();

            builder.Property(t => t.SendUpdatableMeters)
                .IsRequired();
        }
    }
}