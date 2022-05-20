namespace Aristocrat.Mgam.Client.Services.Notification
{
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;

    /// <summary>
    ///     Used for sending notification messages to VLT service.
    /// </summary>
    public interface INotification : IHostService
    {
        /// <summary>
        ///     Sends a notification to the VLT service.
        /// </summary>
        /// <param name="notification">Notification message.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns><see cref="NotificationResponse" />.</returns>
        Task<MessageResult<NotificationResponse>> Send(Notification notification, CancellationToken cancellationToken = default);
    }
}
