namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using CommConfig;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     Configuration for the <see cref="CommHostConfigItem" /> entity
    /// </summary>
    public class CommHostConfigItemMap : IEntityTypeConfiguration<CommHostConfigItem>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CommHostConfigItemMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<CommHostConfigItem> builder)
        {
            builder.ToTable(nameof(CommHostConfigItem));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.HostIndex)
                .IsRequired();

            builder.Property(t => t.HostId)
                .IsRequired();

            builder.Property(t => t.HostLocation)
                .IsRequired();

            builder.Property(t => t.HostRegistered)
                .IsRequired();

            builder.Property(t => t.UseDefaultConfig)
                .IsRequired();

            builder.Property(t => t.RequiredForPlay)
                .IsRequired();

            builder.Property(t => t.TimeToLive)
                .IsRequired();

            builder.Property(t => t.NoResponseTimer)
                .IsRequired();

            builder.Property(t => t.AllowMulticast)
                .IsRequired();

            builder.Property(t => t.CanModLocal)
                .IsRequired();

            builder.Property(t => t.DisplayCommFault)
                .IsRequired();

            builder.Property(t => t.CanModRemote)
                .IsRequired();

            builder.Property(t => t.CommHostConfigId)
                .IsRequired();

            builder.HasMany(x => x.CommHostConfigDevices)
                .WithOne()
                .HasForeignKey(device => device.CommHostConfigItemId);
        }
    }
}