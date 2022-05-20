namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using CommConfig;

    /// <summary>
    ///     Configuration for the <see cref="CommHostConfigItem" /> entity
    /// </summary>
    public class CommHostConfigItemMap : EntityTypeConfiguration<CommHostConfigItem>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CommHostConfigItemMap" /> class.
        /// </summary>
        public CommHostConfigItemMap()
        {
            ToTable("CommHostConfigItem");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.HostIndex)
                .IsRequired();

            Property(t => t.HostId)
                .IsRequired();

            Property(t => t.HostLocation)
                .IsRequired();

            Property(t => t.HostRegistered)
                .IsRequired();

            Property(t => t.UseDefaultConfig)
                .IsRequired();

            Property(t => t.RequiredForPlay)
                .IsRequired();

            Property(t => t.TimeToLive)
                .IsRequired();

            Property(t => t.NoResponseTimer)
                .IsRequired();

            Property(t => t.AllowMulticast)
                .IsRequired();

            Property(t => t.CanModLocal)
                .IsRequired();

            Property(t => t.DisplayCommFault)
                .IsRequired();

            Property(t => t.CanModRemote)
                .IsRequired();

            HasMany(x => x.CommHostConfigDevices)
                .WithRequired()
                .HasForeignKey(device => device.CommHostConfigItemId);
        }
    }
}