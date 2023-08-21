namespace Aristocrat.Mgam.Client.Notification
{
    /// <summary>
    ///     Defines a notification.
    /// </summary>
    public struct NotificationInfo
    {
        /// <summary>
        ///     Gets or sets the numeric identifier for this notification.
        /// </summary>
        public int NotificationId { get; set; }

        /// <summary>
        ///     Gets of sets the human-friendly description of this notification.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the human-friendly description of the notification parameter.
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        ///     Gets are sets the notification priority level.
        /// </summary>
        public NotificationUrgencyLevel UrgencyLevel { get; set; }

        /// <summary>
        ///     Gets are sets the notification recipients.
        /// </summary>
        public NotificationRecipientCode RecipientCodes { get; set; }
    }
}
