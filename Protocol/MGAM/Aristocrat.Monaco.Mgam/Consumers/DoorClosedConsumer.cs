namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Hardware.Contracts.Door;
    using Services.Notification;

    /// <summary>
    ///     Consumes the <see cref="ClosedEvent"/>.
    /// </summary>
    public class DoorClosedConsumer : Consumes<ClosedEvent>
    {
        private readonly IDoorService _doors;
        private readonly INotificationLift _notificationLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DoorClosedConsumer"/> class.
        /// </summary>
        /// <param name="doors"><see cref="IDoorService"/></param>
        /// <param name="notificationLift"><see cref="INotificationLift"/></param>
        public DoorClosedConsumer(
            IDoorService doors,
            INotificationLift notificationLift)
        {
            _doors = doors ?? throw new ArgumentNullException(nameof(doors));
            _notificationLift = notificationLift ?? throw new ArgumentNullException(nameof(notificationLift));
        }

        /// <inheritdoc />
        public override async Task Consume(ClosedEvent theEvent, CancellationToken cancellationToken)
        {
            if (!_doors.LogicalDoors.ContainsKey(theEvent.LogicalId))
            {
                return;
            }

            await _notificationLift.Notify(NotificationCode.DoorClosed, _doors.LogicalDoors[theEvent.LogicalId].Name);
        }
    }
}
