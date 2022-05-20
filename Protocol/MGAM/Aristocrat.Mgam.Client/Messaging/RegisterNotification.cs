namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is used by the VLT to register notifications with VLT service.
    /// </summary>
    public class RegisterNotification : Request, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }

        /// <summary>
        ///     Gets and sets the notification ID.
        /// </summary>
        public int NotificationId { get; set; }

        /// <summary>
        ///     Gets or sets the notification description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the notification priority level.
        /// </summary>
        public int PriorityLevel { get; set; }
    }
}
