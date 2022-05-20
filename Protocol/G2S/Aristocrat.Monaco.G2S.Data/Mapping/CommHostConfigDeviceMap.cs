namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using CommConfig;

    /// <summary>
    ///     Configuration for the <see cref="CommHostConfigDevice" /> entity
    /// </summary>
    public class CommHostConfigDeviceMap : EntityTypeConfiguration<CommHostConfigDevice>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CommHostConfigDeviceMap" /> class.
        /// </summary>
        public CommHostConfigDeviceMap()
        {
            ToTable("CommHostConfigDevice");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.DeviceType)
                .IsRequired();

            Property(t => t.DeviceId)
                .IsRequired();

            Property(t => t.DeviceClass)
                .IsRequired();

            Property(t => t.IsDeviceActive)
                .IsRequired();

            Property(t => t.CanModActiveRemote)
                .IsRequired();

            Property(t => t.CanModOwnerRemote)
                .IsRequired();

            Property(t => t.CanModConfigRemote)
                .IsRequired();
        }
    }
}