namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using Model;

    /// <summary>
    ///     Configuration for the <see cref="MeterSubscription" /> entity
    /// </summary>
    public class MeterSubscriptionMap : EntityTypeConfiguration<MeterSubscription>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MeterSubscriptionMap" /> class.
        /// </summary>
        public MeterSubscriptionMap()
        {
            ToTable("MeterSubscription");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.HostId)
                .IsRequired();

            Property(t => t.SubType)
                .IsRequired();

            Property(t => t.Base)
                .IsRequired();

            Property(t => t.PeriodInterval)
                .IsRequired();

            Property(t => t.DeviceId)
                .IsRequired();

            Property(t => t.MeterType)
                .IsRequired();

            Property(t => t.ClassName)
                .IsRequired();

            Property(t => t.MeterDefinition)
                .IsRequired();

            Property(t => t.OnEndOfDay)
                .IsRequired();

            Property(t => t.OnDoorOpen)
                .IsRequired();

            Property(t => t.OnCoinDrop)
                .IsRequired();

            Property(t => t.OnNoteDrop)
                .IsRequired();

            Property(t => t.LastAckedTime)
                .IsRequired();
        }
    }
}