namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Model;

    /// <summary>
    ///     Configuration for the <see cref="EventHandlerLog" /> entity
    /// </summary>
    public class EventHandlerLogMap : IEntityTypeConfiguration<EventHandlerLog>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EventHandlerLogMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<EventHandlerLog> builder)
        {
            builder.ToTable(nameof(EventHandlerLog));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.HostId)
                .IsRequired();

            builder.Property(t => t.DeviceId)
                .IsRequired();

            builder.Property(t => t.EventId)
                .IsRequired();

            builder.Property(t => t.DeviceClass)
                .IsRequired();

            builder.Property(t => t.EventCode)
                .IsRequired();

            builder.Property(t => t.EventDateTime)
                .IsRequired();

            builder.Property(t => t.TransactionId)
                .IsRequired();

            builder.Property(t => t.EventAck)
                .IsRequired();

            builder.Property(t => t.TransactionList).IsRequired(false);

            builder.Property(t => t.DeviceList).IsRequired(false);

            builder.Property(t => t.MeterList).IsRequired(false);
        }
    }
}