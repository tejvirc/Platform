namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Gaming.Contracts;
    using Services.Notification;

    /// <summary>
    ///     Handles the <see cref="CallAttendantButtonOnEvent" /> event.
    /// </summary>
    public class CallAttendantButtonOnConsumer : Consumes<CallAttendantButtonOnEvent>
    {
        private readonly INotificationLift _notificationLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CallAttendantButtonOnConsumer" /> class.
        /// </summary>
        /// <param name="notificationLift"><see cref="INotificationLift"/></param>
        public CallAttendantButtonOnConsumer(INotificationLift notificationLift)
        {
            _notificationLift = notificationLift ?? throw new ArgumentNullException(nameof(notificationLift));
        }

        /// <inheritdoc />
        public override async Task Consume(CallAttendantButtonOnEvent theEvent, CancellationToken cancellationToken)
        {
            await _notificationLift.Notify(NotificationCode.CallAttendantOn);
        }
    }
}
