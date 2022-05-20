namespace Aristocrat.Monaco.Mgam.Commands
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Services.Registration;
    using AutoMapper;
    using Common;
    using Gaming.Contracts;
    using Kernel;
    using Polly;

    /// <summary>
    ///     Handles the <see cref="RegisterDenominations"/> command.
    /// </summary>
    public class RegisterDenominationsCommandHandler : CommandHandlerBase, ICommandHandler<RegisterDenominations>
    {
        private readonly ILogger<RegisterGamesCommandHandler> _logger;
        private readonly IGameProvider _gameProvider;
        private readonly IEgm _egm;
        private readonly IMapper _mapper;

        /// <summary>
        ///     Initialize a new instance of the <see cref="RegisterGamesCommandHandler"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="egm"><see cref="IEgm"/>.</param>
        /// <param name="mapper"><see cref="IMapper"/>.</param>
        /// <param name="gameProvider"><see cref="IGameProvider"/>.</param>
        /// <param name="bus"><see cref="IEventBus"/>.</param>
        public RegisterDenominationsCommandHandler(
            ILogger<RegisterGamesCommandHandler> logger,
            IEgm egm,
            IMapper mapper,
            IGameProvider gameProvider,
            IEventBus bus) : base(bus)
        {
            _logger = logger;
            _gameProvider = gameProvider;
            _egm = egm;
            _mapper = mapper;
        }

        /// <inheritdoc />
        public async Task Handle(RegisterDenominations command)
        {
            var denominations = _gameProvider.GetConfiguredGames()
                .SelectMany(
                g => g.Denominations,
                (g, d) => new DenomRegistrationInfo
                {
                    GameId = (int)(g.ProductCode ?? g.Id),
                    Denomination = d.Value
                })
                .Distinct()
                .ToArray();
            if (!denominations.Any())
            {
                throw new RegistrationException("No denominations found", RegistrationFailureBehavior.Lock);
            }

            foreach (var denomInfo in denominations)
            {
                var result = await Register(_mapper.Map<RegisterDenomination>(denomInfo));

                if (result.Status != MessageStatus.Success)
                {
                    throw new RegistrationException(
                        $"Comms error occured registering {denomInfo.Denomination} denomination for {denomInfo.GameId}; status: {result.Status}",
                        RegistrationFailureBehavior.Relocate);
                }

                switch (result.Response.ResponseCode)
                {
                    case ServerResponseCode.Ok:
                    case ServerResponseCode.DenominationAlreadyRegistered:
                        _logger.LogDebug(
                            $"Registered {denomInfo.Denomination} denomination for {denomInfo.GameId}; response code: {result.Response.ResponseCode}");
                        break;

                    case ServerResponseCode.InvalidInstanceId:
                    case ServerResponseCode.DeviceStillRegisteredWithVltSvc:
                    case ServerResponseCode.VltServiceNotRegistered:
                        throw new RegistrationException(
                            $"Error occured registering {denomInfo.Denomination} denomination for {denomInfo.GameId}; response code: {result.Response.ResponseCode}",
                            RegistrationFailureBehavior.Relocate);

                    default:
                        throw new RegistrationException(
                            $"Error occured registering {denomInfo.Denomination} denomination for {denomInfo.GameId}; response code: {result.Response.ResponseCode}",
                            RegistrationFailureBehavior.Lock);
                }
            }
        }

        private async Task<MessageResult<RegisterDenominationResponse>> Register(RegisterDenomination message)
        {
            var registration = _egm.GetService<IRegistration>();

            var policy = Policy<MessageResult<RegisterDenominationResponse>>
                .HandleResult(
                    r => r.Status == MessageStatus.Success && r.Response.ResponseCode == ServerResponseCode.ServerError)
                .WaitAndRetryAsync(
                    ProtocolConstants.DefaultRetries,
                    _ => TimeSpan.FromSeconds(ProtocolConstants.DefaultRetryDelay),
                    async (_, retryCount, c) =>
                    {
                        _logger.LogDebug(
                            $"Retrying ({retryCount}) to register denom {message.Denomination} for game {message.GameUpcNumber}.");
                        await Task.CompletedTask;
                    });

            var result = await policy.ExecuteAsync(async () => await registration.Register(message));

            ValidateResponseCode(result.Response);

            return result;
        }
    }
}
