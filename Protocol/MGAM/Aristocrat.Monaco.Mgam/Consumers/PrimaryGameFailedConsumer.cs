namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Gaming.Contracts;
    using Services.Lockup;
    using Services.Notification;

    /// <summary>
    ///     Consumes <see cref="PrimaryGameFailedEvent" /> event.
    /// </summary>
    public class PrimaryGameFailedConsumer : Consumes<PrimaryGameFailedEvent>
    {
        private const string GameMalfunction = "Game Malfunction";
        private readonly ILockup _lockup;
        private readonly INotificationLift _notification;

        /// <summary>
        ///     Construct <see cref="PrimaryGameFailedConsumer" />.
        /// </summary>
        /// <param name="lockup">
        ///     <see cref="ILockup" />
        /// </param>
        /// <param name="notification">
        ///     <see cref="INotificationLift" />
        /// </param>
        public PrimaryGameFailedConsumer(ILockup lockup, INotificationLift notification)
        {
            _lockup = lockup ?? throw new ArgumentNullException(nameof(lockup));
            _notification = notification ?? throw new ArgumentNullException(nameof(notification));
        }

        /// <inheritdoc />
        public override async Task Consume(PrimaryGameFailedEvent evt, CancellationToken cancellationToken)
        {
            _lockup.LockupForEmployeeCard(GameMalfunction);

            await _notification.Notify(NotificationCode.LockedGameMalfunction);
        }
    }
}