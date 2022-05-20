namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using Model;

    /// <summary>
    ///     Configuration for the <see cref="EventHandlerLog" /> entity
    /// </summary>
    public class EventHandlerLogMap : EntityTypeConfiguration<EventHandlerLog>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EventHandlerLogMap" /> class.
        /// </summary>
        public EventHandlerLogMap()
        {
            ToTable("EventHandlerLog");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.HostId)
                .IsRequired();

            Property(t => t.DeviceId)
                .IsRequired();

            Property(t => t.EventId)
                .IsRequired();

            Property(t => t.DeviceClass)
                .IsRequired();

            Property(t => t.EventCode)
                .IsRequired();

            Property(t => t.EventDateTime)
                .IsRequired();

            Property(t => t.TransactionId)
                .IsRequired();

            Property(t => t.EventAck)
                .IsRequired();

            Property(t => t.TransactionList)
                .IsOptional();

            Property(t => t.DeviceList)
                .IsOptional();

            Property(t => t.MeterList)
                .IsOptional();
        }
    }
}