namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     Model configuration for an Host that is stored on the site-controller.
    /// </summary>
    public class HostConfiguration : IEntityTypeConfiguration<Host>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HostConfiguration"/> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<Host> builder)
        {
            builder.ToTable(nameof(Host));

            builder.HasKey(t => t.Id);

            builder.Property(t => t.ServiceName)
                .IsRequired();

            builder.Property(t => t.DirectoryPort)
                .IsRequired();

            builder.Property(t => t.IcdVersion)
                .IsRequired();

            builder.Property(t => t.UseUdpBroadcasting)
                .IsRequired();

            builder.Property(t => t.DirectoryIpAddress)
                .IsRequired();
        }
    }
}
