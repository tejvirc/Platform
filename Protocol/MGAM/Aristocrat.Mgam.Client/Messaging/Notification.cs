namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     Used to notify the VLT service of specific VLT events.
    /// </summary>
    public class Notification : Request, INotification, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }

        /// <summary>
        ///     Gets or sets the notification Id
        /// </summary>
        public int NotificationId { get; set; }

        /// <summary>
        ///     Gets or sets the notification parameter.
        /// </summary>
        public string Parameter { get; set; }
    }
}
