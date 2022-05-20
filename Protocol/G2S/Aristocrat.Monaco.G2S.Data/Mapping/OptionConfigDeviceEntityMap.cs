namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using OptionConfig;

    /// <summary>
    ///     Configuration for the <see cref="OptionConfigDeviceEntity" /> entity
    /// </summary>
    public class OptionConfigDeviceEntityMap : EntityTypeConfiguration<OptionConfigDeviceEntity>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionConfigDeviceEntityMap" /> class.
        /// </summary>
        public OptionConfigDeviceEntityMap()
        {
            ToTable("OptionConfigDevice");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.DeviceClass)
                .IsRequired();

            Property(t => t.DeviceId)
                .IsRequired();

            HasMany(x => x.OptionConfigGroups)
                .WithRequired(x => x.OptionConfigDevice)
                .HasForeignKey(x => x.OptionConfigDeviceId);
        }
    }
}