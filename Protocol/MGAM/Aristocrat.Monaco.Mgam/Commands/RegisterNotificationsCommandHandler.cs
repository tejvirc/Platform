namespace Aristocrat.Monaco.Mgam.Commands
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Notification;
    using Aristocrat.Mgam.Client.Services.Registration;
    using AutoMapper;
    using Kernel;
    using Polly;

    /// <summary>
    ///     Handles the <see cref="RegisterNotifications"/> command.
    /// </summary>
    public class RegisterNotificationsCommandHandler : CommandHandlerBase, ICommandHandler<RegisterNotifications>
    {
        private readonly ILogger _logger;
        private readonly IEgm _egm;
        private readonly IMapper _mapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RegisterNotificationsCommandHandler"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="egm"><see cref="IEgm"/>.</param>
        /// <param name="mapper"><see cref="IMapper"/>.</param>
        /// <param name="bus"><see cref="IEventBus"/>.</param>
        public RegisterNotificationsCommandHandler(
            ILogger<RegisterNotificationsCommandHandler> logger,
            IEgm egm,
            IMapper mapper,
            IEventBus bus) : base(bus)
        {
            _logger = logger;
            _egm = egm;
            _mapper = mapper;
        }

        /// <inheritdoc />
        public async Task Handle(RegisterNotifications command)
        {
            foreach (var notification in SupportedNotifications.Get())
            {
                var result = await Register(_mapper.Map<RegisterNotification>(notification));

                if (result.Status != MessageStatus.Success)
                {
                    throw new RegistrationException(
                        $"Comms error occured registering notification {notification.Description}; status: {result.Status}",
                        RegistrationFailureBehavior.Relocate);
                }

                switch (result.Response.ResponseCode)
                {
                    case ServerResponseCode.Ok:
                    case ServerResponseCode.NotificationAlreadyRegistered:
                        _logger.LogDebug(
                            $"Registered notification {notification.Description}; response code: {result.Response.ResponseCode}");
                        break;

                    case ServerResponseCode.InvalidInstanceId:
                    case ServerResponseCode.DeviceStillRegisteredWithVltSvc:
                    case ServerResponseCode.VltServiceNotRegistered:
                        throw new RegistrationException(
                            $"Error occured registering notification {notification.Description}; response code: {result.Response.ResponseCode}",
                            RegistrationFailureBehavior.Relocate);

                    default:
                        throw new RegistrationException(
                            $"Error occured registering notification {notification.Description}; response code: {result.Response.ResponseCode}",
                            RegistrationFailureBehavior.Lock);
                }
            }
        }

        private async Task<MessageResult<RegisterNotificationResponse>> Register(RegisterNotification message)
        {
            var registration = _egm.GetService<IRegistration>();

            var policy = Policy<MessageResult<RegisterNotificationResponse>>
                .HandleResult(
                    r => r.Status == MessageStatus.Success && r.Response.ResponseCode == ServerResponseCode.ServerError)
                .WaitAndRetryAsync(
                    ProtocolConstants.DefaultRetries,
                    _ => TimeSpan.FromSeconds(ProtocolConstants.DefaultRetryDelay),
                    async (_, retryCount, c) =>
                    {
                        _logger.LogDebug(
                            $"Retrying ({retryCount}) to register notification {message.NotificationId}.");
                        await Task.CompletedTask;
                    });

            var result = await policy.ExecuteAsync(async () => await registration.Register(message));

            ValidateResponseCode(result.Response);

            return result;
        }
    }
}
