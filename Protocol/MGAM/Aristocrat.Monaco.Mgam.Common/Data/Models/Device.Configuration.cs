namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     Model configuration for an Device that is stored on the site-controller.
    /// </summary>
    public class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DeviceConfiguration"/> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            builder.ToTable(nameof(Device));

            builder.HasKey(t => t.Id);

            builder.Property(t => t.DeviceGuid)
                .IsRequired();

            builder.Property(t => t.Name)
                .IsRequired();

            builder.Property(t => t.ManufacturerName)
                .IsRequired();
        }
    }
}
