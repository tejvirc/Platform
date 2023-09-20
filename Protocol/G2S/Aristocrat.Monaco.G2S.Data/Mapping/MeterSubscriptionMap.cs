namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Model;
    using Protocol.Common.Storage;

    /// <summary>
    ///     Configuration for the <see cref="MeterSubscription" /> entity
    /// </summary>
    public class MeterSubscriptionMap : IEntityTypeConfiguration<MeterSubscription>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MeterSubscriptionMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<MeterSubscription> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ToTable(nameof(MeterSubscription));
            builder.HasKey(t => t.Id);
            builder.Property(t => t.HostId)
                .IsRequired();
            builder.Property(t => t.SubType)
                .IsRequired();
            builder.Property(t => t.Base)
                .IsRequired();
            builder.Property(t => t.PeriodInterval)
                .IsRequired();
            builder.Property(t => t.DeviceId)
                .IsRequired();
            builder.Property(t => t.MeterType)
                .IsRequired();
            builder.Property(t => t.ClassName)
                .IsRequired();
            builder.Property(t => t.MeterDefinition)
                .IsRequired();
            builder.Property(t => t.OnEndOfDay)
                .IsRequired();
            builder.Property(t => t.OnDoorOpen)
                .IsRequired();
            builder.Property(t => t.OnCoinDrop)
                .IsRequired();
            builder.Property(t => t.OnNoteDrop)
                .IsRequired();
            builder.Property(t => t.LastAckedTime)
                .IsRequired();
        }
    }
}