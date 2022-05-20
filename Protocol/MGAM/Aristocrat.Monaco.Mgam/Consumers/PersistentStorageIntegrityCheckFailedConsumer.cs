namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Hardware.Contracts.Persistence;
    using Services.Lockup;
    using Services.Notification;

    /// <summary>
    ///     Consumes the <see cref="PersistentStorageIntegrityCheckFailedEvent"/>.
    /// </summary>
    public class PersistentStorageIntegrityCheckFailedConsumer : Consumes<PersistentStorageIntegrityCheckFailedEvent>
    {
        private readonly ILockup _lockup;
        private readonly INotificationLift _notificationLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PersistentStorageIntegrityCheckFailedConsumer"/> class.
        /// </summary>
        /// <param name="lockup"><see cref="ILockup"/></param>
        /// <param name="notificationLift"><see cref="INotificationLift"/></param>
        public PersistentStorageIntegrityCheckFailedConsumer(
            ILockup lockup,
            INotificationLift notificationLift)
        {
            _lockup = lockup ?? throw new ArgumentNullException(nameof(lockup));
            _notificationLift = notificationLift ?? throw new ArgumentNullException(nameof(notificationLift));
        }

        /// <inheritdoc />
        public override async Task Consume(PersistentStorageIntegrityCheckFailedEvent theEvent, CancellationToken cancellationToken)
        {
            _lockup.LockupForEmployeeCard();

            await _notificationLift.Notify(NotificationCode.RamCorruption);
        }
    }
}
