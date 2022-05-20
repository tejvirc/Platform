namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using Model;

    /// <summary>
    ///     Configuration for the <see cref="SupportedEvent" /> entity
    /// </summary>
    public class SupportedEventMap : EntityTypeConfiguration<SupportedEvent>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SupportedEventMap" /> class.
        /// </summary>
        public SupportedEventMap()
        {
            ToTable("SupportedEvent");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.DeviceId)
                .IsRequired();

            Property(t => t.DeviceClass)
                .IsRequired();

            Property(t => t.EventCode)
                .IsRequired();
        }
    }
}