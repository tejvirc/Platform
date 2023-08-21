namespace Aristocrat.Monaco.Mgam.Commands
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Action;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Services.Registration;
    using AutoMapper;
    using Polly;

    /// <summary>
    ///     Handles the <see cref="RegisterActions"/> command.
    /// </summary>
    public class RegisterActionsCommandHandler : ICommandHandler<RegisterActions>
    {
        private readonly ILogger _logger;
        private readonly IEgm _egm;
        private readonly IMapper _mapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RegisterActionsCommandHandler"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="egm"><see cref="IEgm"/>.</param>
        /// <param name="mapper"><see cref="IMapper"/>.</param>
        public RegisterActionsCommandHandler(
            ILogger<RegisterActionsCommandHandler> logger,
            IEgm egm,
            IMapper mapper)
        {
            _logger = logger;
            _egm = egm;
            _mapper = mapper;
        }

        /// <inheritdoc />
        public async Task Handle(RegisterActions command)
        {
            foreach (var action in SupportedActions.Get())
            {
                _logger.LogInfo($"Registering {action.Description} action");

                var result = await Register(_mapper.Map<RegisterAction>(action));

                if (result.Status != MessageStatus.Success)
                {
                    throw new RegistrationException(
                        $"Comms error occurred registering action {action.ActionGuid}; status: {result.Status}",
                        RegistrationFailureBehavior.Relocate);
                }

                switch (result.Response.ResponseCode)
                {
                    case ServerResponseCode.Ok:
                    case ServerResponseCode.ActionAlreadyRegistered:
                        _logger.LogDebug(
                            $"Registered action {action.ActionGuid}; response code: {result.Response.ResponseCode}");
                        break;

                    case ServerResponseCode.InvalidInstanceId:
                    case ServerResponseCode.DeviceStillRegisteredWithVltSvc:
                    case ServerResponseCode.VltServiceNotRegistered:
                        throw new RegistrationException(
                            $"Error occurred registering action {action.ActionGuid}; response code: {result.Response.ResponseCode}",
                            RegistrationFailureBehavior.Relocate);

                    default:
                        throw new RegistrationException(
                            $"Error occurred registering action {action.ActionGuid}; response code: {result.Response.ResponseCode}",
                            RegistrationFailureBehavior.Lock);
                }
            }
        }

        private async Task<MessageResult<RegisterActionResponse>> Register(RegisterAction message)
        {
            var registration = _egm.GetService<IRegistration>();

            var policy = Policy<MessageResult<RegisterActionResponse>>
                .HandleResult(
                    r => r.Status == MessageStatus.Success && r.Response.ResponseCode == ServerResponseCode.ServerError)
                .WaitAndRetryAsync(
                    ProtocolConstants.DefaultRetries,
                    _ => TimeSpan.FromSeconds(ProtocolConstants.DefaultRetryDelay),
                    async (_, retryCount, c) =>
                    {
                        _logger.LogDebug(
                            $"Retrying ({retryCount}) to register action {message.ActionGuid}.");
                        await Task.CompletedTask;
                    });

            return await policy.ExecuteAsync(async () => await registration.Register(message));
        }
    }
}
