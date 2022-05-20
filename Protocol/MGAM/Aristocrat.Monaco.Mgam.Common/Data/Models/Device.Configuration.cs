namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     Model configuration for an Device that is stored on the site-controller.
    /// </summary>
    public class DeviceConfiguration : EntityTypeConfiguration<Device>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DeviceConfiguration"/> class.
        /// </summary>
        public DeviceConfiguration()
        {
            ToTable(nameof(Device));

            HasKey(t => t.Id);

            Property(t => t.DeviceGuid)
                .IsRequired();

            Property(t => t.Name)
                .IsRequired();

            Property(t => t.ManufacturerName)
                .IsRequired();
        }
    }
}
