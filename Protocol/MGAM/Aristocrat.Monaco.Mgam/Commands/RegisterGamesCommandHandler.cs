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
    using Mappings;
    using Polly;

    /// <summary>
    ///     Handles the <see cref="RegisterGames"/> command.
    /// </summary>
    public class RegisterGamesCommandHandler : CommandHandlerBase, ICommandHandler<RegisterGames>
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
        public RegisterGamesCommandHandler(
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
        public async Task Handle(RegisterGames command)
        {
            var games = _gameProvider.GetConfiguredGames().ToArray();
            if (!games.Any())
            {
                throw new RegistrationException("No games found", RegistrationFailureBehavior.Lock);
            }

            foreach (var game in games)
            {
                _logger.LogInfo($"Registering {game.ThemeName} game");

                await RegisterGame(game);
            }
        }

        private async Task RegisterGame(IGameProfile game)
        {
            foreach (var wagerCategory in game.CdsGameInfos)
            {
                var result = await Register(_mapper.MergeInto<RegisterGame>(game, wagerCategory));

                if (result.Status != MessageStatus.Success)
                {
                    throw new RegistrationException(
                        $"Comms error occured registering game {game.ThemeName}; status: {result.Status}",
                        RegistrationFailureBehavior.Relocate);
                }

                switch (result.Response.ResponseCode)
                {
                    case ServerResponseCode.Ok:
                    case ServerResponseCode.GameAlreadyRegistered:
                        _logger.LogDebug(
                            $"Registered game {game.ThemeName}; response code: {result.Response.ResponseCode}");
                        break;

                    case ServerResponseCode.InvalidInstanceId:
                    case ServerResponseCode.DeviceStillRegisteredWithVltSvc:
                    case ServerResponseCode.VltServiceNotRegistered:
                        throw new RegistrationException(
                            $"Error occured registering game {game.ThemeName}; response code: {result.Response.ResponseCode}",
                            RegistrationFailureBehavior.Relocate);

                    default:
                        throw new RegistrationException(
                            $"Error occured registering game {game.ThemeName}; response code: {result.Response.ResponseCode}",
                            RegistrationFailureBehavior.Lock);
                }
            }
        }

        private async Task<MessageResult<RegisterGameResponse>> Register(RegisterGame message)
        {
            var registration = _egm.GetService<IRegistration>();

            var policy = Policy<MessageResult<RegisterGameResponse>>
                .HandleResult(
                    r => r.Status == MessageStatus.Success && r.Response.ResponseCode == ServerResponseCode.ServerError)
                .WaitAndRetryAsync(
                    ProtocolConstants.DefaultRetries,
                    _ => TimeSpan.FromSeconds(ProtocolConstants.DefaultRetryDelay),
                    async (_, retryCount, c) =>
                    {
                        _logger.LogDebug(
                            $"Retrying ({retryCount}) to register game {message.GameDescription}.");
                        await Task.CompletedTask;
                    });

            var result = await policy.ExecuteAsync(async () => await registration.Register(message));

            ValidateResponseCode(result.Response);

            return result;
        }
    }
}
