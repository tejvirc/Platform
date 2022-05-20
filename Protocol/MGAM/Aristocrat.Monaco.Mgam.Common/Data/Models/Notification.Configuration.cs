namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     Model configuration for an Notification that is stored on the site-controller.
    /// </summary>
    public class NotificationConfiguration : EntityTypeConfiguration<Notification>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NotificationConfiguration"/> class.
        /// </summary>
        public NotificationConfiguration()
        {
            ToTable(nameof(Notification));

            HasKey(t => t.Id);

            Property(t => t.NotificationId)
                .IsRequired();

            Property(t => t.Parameter);
        }
    }
}
