namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts.Identification;
    using Aristocrat.Mgam.Client;
    using Hardware.Contracts.Door;
    using Services.DropMode;
    using Services.Lockup;
    using Services.Notification;

    /// <summary>
    ///     Consumes the <see cref="OpenEvent"/>.
    /// </summary>
    public class DoorOpenConsumer : Consumes<OpenEvent>
    {
        private readonly IDoorService _doors;
        private readonly IEmployeeLogin _employeeLogin;
        private readonly ILockup _lockup;
        private readonly INotificationLift _notificationLift;
        private readonly IDropMode _dropMode;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DoorOpenConsumer"/> class.
        /// </summary>
        /// <param name="doors"><see cref="IDoorService"/></param>
        /// <param name="employeeLogin"><see cref="IEmployeeLogin"/></param>
        /// <param name="lockup"><see cref="ILockup"/></param>
        /// <param name="notificationLift"><see cref="INotificationLift"/></param>
        /// <param name="dropMode"><see cref="IDropMode"/>.</param>
        public DoorOpenConsumer(
            IDoorService doors,
            IEmployeeLogin employeeLogin,
            ILockup lockup,
            INotificationLift notificationLift,
            IDropMode dropMode)
        {
            _doors = doors ?? throw new ArgumentNullException(nameof(doors));
            _employeeLogin = employeeLogin ?? throw new ArgumentNullException(nameof(employeeLogin));
            _lockup = lockup ?? throw new ArgumentNullException(nameof(lockup));
            _notificationLift = notificationLift ?? throw new ArgumentNullException(nameof(notificationLift));
            _dropMode = dropMode ?? throw new ArgumentNullException(nameof(dropMode));
        }

        /// <inheritdoc />
        public override async Task Consume(OpenEvent theEvent, CancellationToken cancellationToken)
        {
            if (!_doors.LogicalDoors.ContainsKey(theEvent.LogicalId))
            {
                return;
            }

            var logicalDoor = _doors.LogicalDoors[theEvent.LogicalId];

            if (_dropMode.Active)
            {
                await _notificationLift.Notify(NotificationCode.DoorOpened, logicalDoor.Name);
                return;
            }

            _lockup.LockupForEmployeeCard();

            await _notificationLift.Notify(
                _employeeLogin.IsLoggedIn ? NotificationCode.DoorOpened : NotificationCode.LockedDoorOpen,
                logicalDoor.Name);
        }
    }
}
