namespace Aristocrat.Monaco.Mgam.Services.Notification
{
    using Aristocrat.Mgam.Client.Messaging;

    /// <summary>
    ///     Used to queue offline notifications.
    /// </summary>
    public interface INotificationQueue
    {
        /// <summary>
        ///     Adds an notification to the end of the queue/database.
        /// </summary>
        /// <param name="notification">The notification to add to the end of the queue/database.</param>
        void Enqueue(Notification notification);

        /// <summary>
        ///     Tries to remove and return the notification at the beginning of the queue/database.
        /// </summary>
        /// <returns>True if there is a notification in the queue.</returns>
        bool TryDequeue(out Notification notification);

        /// <summary>
        ///     Tries to return an notification from the beginning of the queue/database without removing it.
        /// </summary>
        /// <returns>True if an notification was returned successfully; otherwise, false.</returns>
        bool TryPeek(out Notification notification);

        /// <summary>
        ///     Removes all notifications from the queue/database.
        /// </summary>
        void Clear();
    }
}
