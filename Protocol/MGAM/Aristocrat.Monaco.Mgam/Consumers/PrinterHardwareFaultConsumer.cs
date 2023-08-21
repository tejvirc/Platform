namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Hardware.Contracts.Printer;
    using Services.Lockup;
    using Services.Notification;

    /// <summary>
    ///     Handles the Printer <see cref="HardwareFaultEvent" /> event.
    /// </summary>
    public class PrinterHardwareFaultConsumer : Consumes<HardwareFaultEvent>
    {
        private readonly ILockup _lockup;
        private readonly INotificationLift _notificationLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrinterHardwareFaultConsumer"/> class.
        /// </summary>
        /// <param name="lockup"><see cref="ILockup"/></param>
        /// <param name="notificationLift"><see cref="INotificationLift"/></param>
        public PrinterHardwareFaultConsumer(
            ILockup lockup,
            INotificationLift notificationLift)
        {
            _lockup = lockup ?? throw new ArgumentNullException(nameof(lockup));
            _notificationLift = notificationLift ?? throw new ArgumentNullException(nameof(notificationLift));
        }

        /// <inheritdoc />
        public override async Task Consume(HardwareFaultEvent theEvent, CancellationToken cancellationToken)
        {
            foreach (PrinterFaultTypes flag in Enum.GetValues(typeof(PrinterFaultTypes)))
            {
                if (flag == PrinterFaultTypes.None || !theEvent.Fault.HasFlag(flag))
                {
                    continue;
                }

                _lockup.LockupForEmployeeCard(); // this won't lock up if employee card is already present

                switch (flag)
                {
                    case PrinterFaultTypes.PaperEmpty:
                        await _notificationLift.Notify(NotificationCode.LockedPrinterOutOfPaper);
                        break;

                    case PrinterFaultTypes.PaperJam:
                        await _notificationLift.Notify(NotificationCode.LockedPrinterJammed);
                        break;

                    default:
                        await _notificationLift.Notify(NotificationCode.LockedTilt, flag.ToString());
                        break;
                }
            }
        }
    }
}
