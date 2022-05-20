namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using Model;

    /// <summary>
    ///     Configuration for the <see cref="EventSubscription" /> entity
    /// </summary>
    public class EventSubscriptionMap : EntityTypeConfiguration<EventSubscription>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EventSubscriptionMap" /> class.
        /// </summary>
        public EventSubscriptionMap()
        {
            ToTable("EventSubscription");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.HostId)
                .IsRequired();

            Property(t => t.DeviceId)
                .IsRequired();

            Property(t => t.EventCode)
                .IsRequired();

            Property(t => t.SubType)
                .IsRequired();

            Property(t => t.DeviceClass)
                .IsRequired();

            Property(t => t.EventPersist)
                .IsRequired();

            Property(t => t.SendClassMeters)
                .IsRequired();

            Property(t => t.SendDeviceMeters)
                .IsRequired();

            Property(t => t.SendDeviceStatus)
                .IsRequired();


            Property(t => t.SendTransaction)
                .IsRequired();

            Property(t => t.SendUpdatableMeters)
                .IsRequired();
        }
    }
}