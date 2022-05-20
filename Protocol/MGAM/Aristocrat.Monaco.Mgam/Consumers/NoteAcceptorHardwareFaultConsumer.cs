namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Commands;
    using Hardware.Contracts.NoteAcceptor;
    using Services.DropMode;
    using Services.Lockup;
    using Services.Notification;

    /// <summary>
    ///     Handles the Note Acceptor <see cref="HardwareFaultEvent" /> event.
    /// </summary>
    public class NoteAcceptorHardwareFaultConsumer : Consumes<HardwareFaultEvent>
    {
        private readonly ILockup _lockup;
        private readonly ILogger _logger;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly INotificationLift _notificationLift;
        private readonly IDropMode _dropMode;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorHardwareFaultConsumer"/> class.
        /// </summary>
        /// <param name="lockup"><see cref="ILockup"/></param>
        /// <param name="logger"><see cref="ILogger" />.</param>
        /// <param name="commandFactory">Instance of <see cref="ICommandHandlerFactory"/>.</param>
        /// <param name="notificationLift"><see cref="INotificationLift"/></param>
        /// <param name="dropMode"><see cref="IDropMode"/></param>
        public NoteAcceptorHardwareFaultConsumer(
            ILockup lockup,
            ILogger<NoteAcceptorHardwareFaultClearConsumer> logger,
            ICommandHandlerFactory commandFactory,
            INotificationLift notificationLift,
            IDropMode dropMode)
        {
            _lockup = lockup ?? throw new ArgumentNullException(nameof(lockup));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _notificationLift = notificationLift ?? throw new ArgumentNullException(nameof(notificationLift));
            _dropMode = dropMode ?? throw new ArgumentNullException(nameof(dropMode));
        }

        /// <inheritdoc />
        public override async Task Consume(HardwareFaultEvent theEvent, CancellationToken cancellationToken)
        {
            // Commands
            if (_dropMode.Active && theEvent.Fault.HasFlag(NoteAcceptorFaultTypes.StackerDisconnected))
            {
                try
                {
                    _logger.LogInfo("NoteAcceptor Stacker Reconnected during Drop Mode causes BillAcceptorMeterReport");
                    await _commandFactory.Execute(new Commands.BillAcceptorMeterReport());
                }
                catch (ServerResponseException ex)
                {
                    _logger.LogError(ex, "NoteAcceptor Stacker Reconnected BillAcceptorMeterReport failed ServerResponseException");
                }
            }

            // Notifications
            foreach (NoteAcceptorFaultTypes flag in Enum.GetValues(typeof(NoteAcceptorFaultTypes)))
            {
                if (flag == NoteAcceptorFaultTypes.None || !theEvent.Fault.HasFlag(flag))
                {
                    continue;
                }

                if (!_dropMode.Active)
                {
                    _lockup.LockupForEmployeeCard(priority: Kernel.SystemDisablePriority.Normal); // this won't lock up if employee card is already present
                }

                switch (flag)
                {
                    case NoteAcceptorFaultTypes.StackerJammed:
                    case NoteAcceptorFaultTypes.NoteJammed:
                        await _notificationLift.Notify(NotificationCode.LockedBillAcceptorJam);
                        break;

                    case NoteAcceptorFaultTypes.StackerFull:
                        await _notificationLift.Notify(NotificationCode.LockedBillAcceptorFull);
                        break;

                    default:
                        await _notificationLift.Notify(NotificationCode.LockedTilt, flag.ToString());
                        break;
                }
            }
        }
    }
}
