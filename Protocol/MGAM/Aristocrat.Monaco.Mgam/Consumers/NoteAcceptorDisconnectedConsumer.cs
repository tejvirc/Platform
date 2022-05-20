namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Services.Lockup;
    using Services.Notification;

    /// <summary>
    ///     Handles the Note Acceptor <see cref="DisconnectedEvent" /> event.
    /// </summary>
    public class NoteAcceptorDisconnectedConsumer : Consumes<DisconnectedEvent>
    {
        private readonly ILockup _lockup;
        private readonly ILogger _logger;
        private readonly INotificationLift _notificationLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorDisconnectedConsumer" /> class.
        /// </summary>
        /// <param name="lockup">
        ///     <see cref="ILockup" />
        /// </param>
        /// <param name="logger"><see cref="ILogger" />.</param>
        /// <param name="notificationLift">
        ///     <see cref="INotificationLift" />
        /// </param>
        public NoteAcceptorDisconnectedConsumer(
            ILockup lockup,
            ILogger<NoteAcceptorDisconnectedConsumer> logger,
            INotificationLift notificationLift)
        {
            _lockup = lockup ?? throw new ArgumentNullException(nameof(lockup));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationLift = notificationLift ?? throw new ArgumentNullException(nameof(notificationLift));
        }

        /// <inheritdoc />
        public override async Task Consume(DisconnectedEvent theEvent, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Employee login required when BNA disconnected");
            _lockup.LockupForEmployeeCard(priority: SystemDisablePriority.Normal);

            await _notificationLift.Notify(NotificationCode.LockedTilt, theEvent.ToString());
        }
    }
}