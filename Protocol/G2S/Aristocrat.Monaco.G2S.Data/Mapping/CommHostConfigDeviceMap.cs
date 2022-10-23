namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using CommConfig;
    using System.Reflection.Emit;

    /// <summary>
    ///     Configuration for the <see cref="CommHostConfigDevice" /> entity
    /// </summary>
    public class CommHostConfigDeviceMap : IEntityTypeConfiguration<CommHostConfigDevice>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CommHostConfigDeviceMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<CommHostConfigDevice> builder)
        {
            builder.ToTable(nameof(CommHostConfigDevice));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.DeviceType)
                .IsRequired();

            builder.Property(t => t.DeviceId)
                .IsRequired();

            builder.Property(t => t.DeviceClass)
                .IsRequired();

            builder.Property(t => t.IsDeviceActive)
                .IsRequired();

            builder.Property(t => t.CanModActiveRemote)
                .IsRequired();

            builder.Property(t => t.CanModOwnerRemote)
                .IsRequired();

            builder.Property(t => t.CanModConfigRemote)
            .IsRequired();
        }
    }
}