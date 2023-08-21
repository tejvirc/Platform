namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Hardware.Contracts.Persistence;
    using Services.Notification;

    /// <summary>
    ///     Handles the <see cref="StorageErrorEvent" /> event.
    /// </summary>
    public class StorageErrorConsumer : Consumes<StorageErrorEvent>
    {
        private readonly INotificationLift _notificationLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StorageErrorConsumer" /> class.
        /// </summary> 
        /// <param name="notificationLift"><see cref="INotificationLift"/></param>
        public StorageErrorConsumer(INotificationLift notificationLift)
        {
            _notificationLift = notificationLift ?? throw new ArgumentNullException(nameof(notificationLift));
        }

        /// <inheritdoc />
        public override async Task Consume(StorageErrorEvent theEvent, CancellationToken cancellationToken)
        {
            var failureType = theEvent.Id.ToString();
            await _notificationLift.Notify(NotificationCode.RamCorruption, failureType);
        }
    }
}
