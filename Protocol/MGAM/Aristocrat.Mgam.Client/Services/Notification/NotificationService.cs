namespace Aristocrat.Mgam.Client.Services.Notification
{
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;
    using Registration;
    using Routing;

    /// <summary>
    ///     This class is used to send notification messages to VLT service.
    /// </summary>
    internal class NotificationService : INotification
    {
        private readonly IRequestRouter _router;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RegistrationService"/> class.
        /// </summary>
        /// <param name="router"><see cref="IRequestRouter"/>.</param>
        /// <param name="services"><see cref="IHostServiceCollection"/>.</param>
        public NotificationService(
            IRequestRouter router,
            IHostServiceCollection services)
        {
            _router = router;

            services.Add(this);
        }

        /// <inheritdoc />
        public async Task<MessageResult<NotificationResponse>> Send(
            Notification message,
            CancellationToken cancellationToken)
        {
            return await _router.Send<Notification, NotificationResponse>(message, cancellationToken);
        }
    }
}
