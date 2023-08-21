namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using Monaco.Common.Storage;

    /// <summary>
    ///     Model for the Notification.
    /// </summary>
    public class Notification : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the notification ID.
        /// </summary>
        public int NotificationId { get; set; }

        /// <summary>
        ///     Gets or sets the notification parameter.
        /// </summary>
        public string Parameter { get; set; }
    }
}
