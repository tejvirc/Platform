namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     Model configuration for an Host that is stored on the site-controller.
    /// </summary>
    public class HostConfiguration : EntityTypeConfiguration<Host>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HostConfiguration"/> class.
        /// </summary>
        public HostConfiguration()
        {
            ToTable(nameof(Host));

            HasKey(t => t.Id);

            Property(t => t.ServiceName)
                .IsRequired();

            Property(t => t.DirectoryPort)
                .IsRequired();

            Property(t => t.IcdVersion)
                .IsRequired();

            Property(t => t.UseUdpBroadcasting)
                .IsRequired();

            Property(t => t.DirectoryIpAddress)
                .IsRequired();
        }
    }
}
