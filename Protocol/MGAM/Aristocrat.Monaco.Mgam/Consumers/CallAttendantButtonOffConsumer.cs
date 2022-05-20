namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Gaming.Contracts;
    using Services.Notification;

    /// <summary>
    ///     Handles the <see cref="CallAttendantButtonOffEvent" />
    /// </summary>
    public class CallAttendantButtonOffConsumer : Consumes<CallAttendantButtonOffEvent>
    {
        private readonly INotificationLift _notificationLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CallAttendantButtonOffConsumer" /> class.
        /// </summary>
        /// <param name="notificationLift"><see cref="INotificationLift"/></param>
        public CallAttendantButtonOffConsumer(INotificationLift notificationLift)
        {
            _notificationLift = notificationLift ?? throw new ArgumentNullException(nameof(notificationLift));
        }

        /// <inheritdoc />
        public override async Task Consume(CallAttendantButtonOffEvent theEvent, CancellationToken cancellationToken)
        {
            await _notificationLift.Notify(NotificationCode.CallAttendantOff);
        }
    }
}
