namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Kernel;
    using Services.Notification;

    /// <summary>
    ///     Consumes <see cref="PlatformBootedEvent" /> event.
    /// </summary>
    public class PlatformBootedConsumer : Consumes<PlatformBootedEvent>
    {
        private readonly INotificationLift _notificationLift;

        /// <summary>
        ///     Construct <see cref="PlatformBootedConsumer" />.
        /// </summary>
        /// <param name="notificationLift">
        ///     <see cref="INotificationLift" />
        /// </param>
        public PlatformBootedConsumer(INotificationLift notificationLift)
        {
            _notificationLift = notificationLift ?? throw new ArgumentNullException(nameof(notificationLift));
        }

        /// <inheritdoc />
        public override async Task Consume(PlatformBootedEvent evt, CancellationToken cancellationToken)
        {
            await _notificationLift.Notify(NotificationCode.PowerFailure);
        }
    }
}