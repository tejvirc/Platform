namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.Threading.Tasks;
    using Application.Contracts.Extensions;
    using Aristocrat.Mgam.Client;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Services.Notification;

    /// <summary>
    ///     Handles the <see cref="LinkedProgressiveHitEvent" /> event.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class LinkedProgressiveHitConsumer : IProtocolProgressiveEventHandler
    {
        private readonly INotificationLift _notificationLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LinkedProgressiveHitConsumer" /> class.
        /// </summary>
        /// <param name="notificationLift">
        ///     <see cref="INotificationLift" />
        /// </param>
        public LinkedProgressiveHitConsumer(INotificationLift notificationLift)
        {
            _notificationLift = notificationLift ?? throw new ArgumentNullException(nameof(notificationLift));
        }

        private async Task Consume(LinkedProgressiveHitEvent theEvent)
        {
            await _notificationLift.Notify(
                NotificationCode.ProgressiveJackpot,
                $"{theEvent.Level.ProgressivePackName}_{theEvent.Level.ResetValue.MillicentsToDollars()}_{theEvent.Level.LevelName}");
        }

        public void HandleProgressiveEvent<T>(T data)
        {
            Consume(data as LinkedProgressiveHitEvent).Wait();
        }
    }
}