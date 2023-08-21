namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     Model configuration for an Notification that is stored on the site-controller.
    /// </summary>
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NotificationConfiguration"/> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable(nameof(Notification));

            builder.HasKey(t => t.Id);

            builder.Property(t => t.NotificationId)
                .IsRequired();

            builder.Property(t => t.Parameter);
        }
    }
}
