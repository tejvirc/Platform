namespace Aristocrat.Monaco.Mgam.Commands
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Command;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Services.Registration;
    using AutoMapper;
    using Kernel;
    using Polly;

    /// <summary>
    ///     Handles the <see cref="RegisterCommands"/> command.
    /// </summary>
    public class RegisterCommandsCommandHandler : CommandHandlerBase, ICommandHandler<RegisterCommands>
    {
        private readonly ILogger _logger;
        private readonly IEgm _egm;
        private readonly IMapper _mapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RegisterCommandsCommandHandler"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="egm"><see cref="IEgm"/>.</param>
        /// <param name="mapper"><see cref="IMapper"/>.</param>
        /// <param name="bus"><see cref="IEventBus"/>.</param>
        public RegisterCommandsCommandHandler(
            ILogger<RegisterCommandsCommandHandler> logger,
            IEgm egm,
            IMapper mapper,
            IEventBus bus) : base(bus)
        {
            _logger = logger;
            _egm = egm;
            _mapper = mapper;
        }

        /// <inheritdoc />
        public async Task Handle(RegisterCommands command)
        {
            foreach (var cmd in SupportedCommands.Get())
            {
                var result = await Register(_mapper.Map<RegisterCommand>(cmd));

                if (result.Status != MessageStatus.Success)
                {
                    throw new RegistrationException(
                        $"Comms error occured registering command {cmd.Description}; status: {result.Status}",
                        RegistrationFailureBehavior.Relocate);
                }

                switch (result.Response.ResponseCode)
                {
                    case ServerResponseCode.Ok:
                    case ServerResponseCode.CommandAlreadyRegistered:
                        _logger.LogDebug(
                            $"Registered command {cmd.Description}; response code: {result.Response.ResponseCode}");
                        break;

                    case ServerResponseCode.InvalidInstanceId:
                    case ServerResponseCode.DeviceStillRegisteredWithVltSvc:
                    case ServerResponseCode.VltServiceNotRegistered:
                        throw new RegistrationException(
                            $"Error occured registering command {cmd.Description}; response code: {result.Response.ResponseCode}",
                            RegistrationFailureBehavior.Relocate);

                    default:
                        throw new RegistrationException(
                            $"Error occured registering command {cmd.Description}; response code: {result.Response.ResponseCode}",
                            RegistrationFailureBehavior.Lock);
                }
            }
        }

        private async Task<MessageResult<RegisterCommandResponse>> Register(RegisterCommand message)
        {
            var registration = _egm.GetService<IRegistration>();

            var policy = Policy<MessageResult<RegisterCommandResponse>>
                .HandleResult(
                    r => r.Status == MessageStatus.Success && r.Response.ResponseCode == ServerResponseCode.ServerError)
                .WaitAndRetryAsync(
                    ProtocolConstants.DefaultRetries,
                    _ => TimeSpan.FromSeconds(ProtocolConstants.DefaultRetryDelay),
                    async (_, retryCount, c) =>
                    {
                        _logger.LogDebug(
                            $"Retrying ({retryCount}) to register command {message.CommandId}.");
                        await Task.CompletedTask;
                    });

            var result = await policy.ExecuteAsync(async () => await registration.Register(message));

            ValidateResponseCode(result.Response);

            return result;
        }
    }
}
