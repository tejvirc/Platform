namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using OptionConfig;

    /// <summary>
    ///     Configuration for the <see cref="OptionConfigDeviceEntity" /> entity
    /// </summary>
    public class OptionConfigDeviceEntityMap : IEntityTypeConfiguration<OptionConfigDeviceEntity>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionConfigDeviceEntityMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<OptionConfigDeviceEntity> builder)
        {
            builder.ToTable("OptionConfigDevice");

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.DeviceClass)
                .IsRequired();

            builder.Property(t => t.DeviceId)
                .IsRequired();

            builder.HasMany(x => x.OptionConfigGroups)
                .WithOne(x => x.OptionConfigDevice)
                .HasForeignKey(x => x.OptionConfigDeviceId);
        }
    }
}