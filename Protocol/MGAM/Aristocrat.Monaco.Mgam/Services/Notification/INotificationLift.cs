namespace Aristocrat.Monaco.Mgam.Services.Notification
{
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;

    /// <summary>
    ///     Sends notifications to the VLT service.
    /// </summary>
    public interface INotificationLift
    {
        /// <summary>
        ///     Sends a notification to the host.
        /// </summary>
        /// <param name="code">The notification code.</param>
        /// <param name="parameter">Parameter as required by the notification identifier.</param>
        /// <returns><see cref="Task"/>.</returns>
        Task Notify(NotificationCode code, string parameter = null);

        /// <summary>
        ///     Sends a notification to the host.
        /// </summary>
        /// <param name="id">The notification identifier.</param>
        /// <param name="parameter">Parameter as required by the notification identifier.</param>
        /// <returns><see cref="Task"/>.</returns>
        Task Notify(int id, string parameter = null);

        /// <summary>
        ///     Suspend sending notifications.
        /// </summary>
        void Suspend();

        /// <summary>
        ///     Continue sending notifications.
        /// </summary>
        Task Continue();
    }
}
