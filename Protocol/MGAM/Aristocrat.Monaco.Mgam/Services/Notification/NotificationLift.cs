namespace Aristocrat.Monaco.Mgam.Services.Notification
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Common;
    using Common.Events;
    using Kernel;
    using Monaco.Common;
    using INotification = Aristocrat.Mgam.Client.Services.Notification.INotification;

    /// <summary>
    ///     Sends notifications to the VLT service.
    /// </summary>
    internal class NotificationLift : INotificationLift
    {
        private readonly ILogger _logger;
        private readonly IEventBus _eventBus;
        private readonly IEgm _egm;
        private readonly INotificationQueue _queue;

        private bool _suspended = true;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NotificationLift"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger{TCategory}"/>.</param>
        /// <param name="eventBus"><see cref="IEventBus"/>.</param>
        /// <param name="egm"><see cref="IEgm"/>.</param>
        /// <param name="queue"><see cref="INotificationQueue"/>.</param>
        public NotificationLift(
            ILogger<NotificationLift> logger,
            IEventBus eventBus,
            IEgm egm,
            INotificationQueue queue)
        {
            _logger = logger;
            _eventBus = eventBus;
            _egm = egm;
            _queue = queue;
        }

        /// <inheritdoc />
        public Task Notify(NotificationCode code, string parameter = null)
        {
            return Notify((int)code, parameter);
        }

        /// <inheritdoc />
        public async Task Notify(int id, string parameter = null)
        {
            await Notify(new Notification { NotificationId = id, Parameter = parameter ?? string.Empty });
        }

        public void Suspend()
        {
            _suspended = true;
        }

        public async Task Continue()
        {
            _suspended = false;

            await Flush();
        }

        private async Task Notify(Notification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            Task.Run(
                async () =>
                {
                    if (_suspended)
                    {
                        await Postpone(notification);
                        return;
                    }

                    if (!await Flush())
                    {
                        await Postpone(notification);
                        return;
                    }

                    var result = await Deliver(notification);
                    if (result.Response?.ResponseCode != ServerResponseCode.Ok)
                    {
                        await Postpone(notification);
                        return;
                    }

                    _logger.LogDebug($"Sent notification {notification.NotificationId}");
                }
            ).FireAndForget(
                ex => _logger.LogError($"Error sending notification {notification.NotificationId}", ex));

            await Task.CompletedTask;
        }

        private async Task<MessageResult<NotificationResponse>> Deliver(Notification notification)
        {
            try
            {
                var result = await _egm.GetService<INotification>().Send(notification);

                if (result.Response != null)
                {
                    switch (result.Response.ResponseCode)
                    {
                        case ServerResponseCode.InvalidInstanceId:
                        case ServerResponseCode.VltServiceNotRegistered:
                        case ServerResponseCode.ServerError:
                            _eventBus.Publish(new ForceDisconnectEvent(DisconnectReason.InvalidServerResponse));
                            break;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarn(ex, $"Could not send notification {notification.NotificationId}");
                return MessageResult<NotificationResponse>.Create(MessageStatus.UnknownError);
            }
        }

        private async Task Postpone(Notification notification)
        {
            _queue.Enqueue(notification);
            _logger.LogDebug($"Queued notification {notification.NotificationId}");

            await Task.CompletedTask;
        }

        private async Task<bool> Flush()
        {
            while (_queue.TryPeek(out var notification))
            {
                var result = await Deliver(notification);
                if (result.Response?.ResponseCode == ServerResponseCode.Ok)
                {
                    _queue.TryDequeue(out _);
                    _logger.LogDebug($"Sent queued notification {notification.NotificationId}");
                }
                else
                {
                    _logger.LogWarn($"Send queued notification {notification.NotificationId} failure");
                    return false;
                }
            }

            return true;
        }
    }
}
