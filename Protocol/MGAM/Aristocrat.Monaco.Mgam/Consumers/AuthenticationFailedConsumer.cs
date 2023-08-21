namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts.Authentication;
    using Aristocrat.Mgam.Client;
    using Services.Lockup;
    using Services.Notification;

    /// <summary>
    ///     Consumes the <see cref="LiveAuthenticationFailedEvent"/>.
    /// </summary>
    public class AuthenticationFailedConsumer : Consumes<LiveAuthenticationFailedEvent>
    {
        private readonly ILockup _lockup;
        private readonly INotificationLift _notificationLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuthenticationFailedConsumer"/> class.
        /// </summary>
        /// <param name="lockup"><see cref="ILockup"/></param>
        /// <param name="notificationLift"><see cref="INotificationLift"/></param>
        public AuthenticationFailedConsumer(
            ILockup lockup,
            INotificationLift notificationLift)
        {
            _lockup = lockup ?? throw new ArgumentNullException(nameof(lockup));
            _notificationLift = notificationLift ?? throw new ArgumentNullException(nameof(notificationLift));
        }

        /// <inheritdoc />
        public override async Task Consume(LiveAuthenticationFailedEvent theEvent, CancellationToken cancellationToken)
        {
            _lockup.LockupForEmployeeCard();

            await _notificationLift.Notify(NotificationCode.LockedSoftwareError, theEvent.Message);
        }
    }
}
