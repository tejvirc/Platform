namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts.Localization;
    using Aristocrat.Mgam.Client;
    using Hardware.Contracts.Battery;
    using Localization.Properties;
    using Services.Lockup;
    using Services.Notification;

    /// <summary>
    ///     Handles the <see cref="BatteryLowEvent" /> event.
    /// </summary>
    public class BatteryLowConsumer : Consumes<BatteryLowEvent>
    {
        private readonly ILockup _lockup;
        private readonly INotificationLift _notificationLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BatteryLowConsumer" /> class.
        /// </summary>
        /// <param name="lockup"><see cref="ILockup"/></param>
        /// <param name="notificationLift"><see cref="INotificationLift"/></param>
        public BatteryLowConsumer(ILockup lockup, INotificationLift notificationLift)
        {
            _lockup = lockup ?? throw new ArgumentNullException(nameof(lockup));
            _notificationLift = notificationLift ?? throw new ArgumentNullException(nameof(notificationLift));
        }

        /// <inheritdoc />
        public override async Task Consume(BatteryLowEvent theEvent, CancellationToken cancellationToken)
        {
            _lockup.LockupForEmployeeCard(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BatteryLowTilt));

            await _notificationLift.Notify(NotificationCode.LowRamBattery, theEvent.BatteryId.ToString());
        }
    }
}
