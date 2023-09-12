namespace Aristocrat.Monaco.Bingo.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Common;
    using Common.Exceptions;
    using Gaming.Contracts;
    using Kernel;
    using Protocol.Common.Storage.Entity;

    public class ProgressiveRegistrationCommandHandler : IProgressiveCommandHandler<ProgressiveRegistrationCommand>
    {
        private readonly IProgressiveRegistrationService _registrationService;
        private readonly IPropertiesManager _properties;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IGameProvider _gameProvider;

        public ProgressiveRegistrationCommandHandler(
            IProgressiveRegistrationService registrationService,
            IPropertiesManager properties,
            IUnitOfWorkFactory unitOfWorkFactory,
            IGameProvider gameProvider)
        {
            _registrationService = registrationService ?? throw new ArgumentNullException(nameof(registrationService));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        public async Task Handle(ProgressiveRegistrationCommand command, CancellationToken token = default)
        {
            try
            {
                var machineId = _properties.GetValue(ApplicationConstants.MachineId, (uint)0).ToString();
                var gameConfiguration = _unitOfWorkFactory.GetSelectedGameConfiguration(_gameProvider);
                var gameTitleId = (int)(gameConfiguration?.GameTitleId ?? 0);

                // After an nvram clear there will be no active game set at registration time. In this case use the first enabled game.
                var currentGame = _gameProvider.GetGame(_properties.GetValue(GamingConstants.SelectedGameId, 0)) ?? _gameProvider.GetEnabledGames().First();

                if (currentGame != null)
                {
                    var subGames = _gameProvider.GetEnabledSubGames(currentGame);
                    var progressiveGames = new List<ProgressiveGameRegistrationData>();
                    foreach (var game in subGames)
                    {
                        foreach (var denom in game.Denominations)
                        {
                            // Setting MaxBet equal to the denom
                            progressiveGames.Add(
                                new ProgressiveGameRegistrationData(
                                    int.Parse(game.CdsTitleId),
                                    (int)denom.Value,
                                    (int)denom.Value));
                        }
                    }

                    var message = new ProgressiveRegistrationMessage(machineId, gameTitleId, progressiveGames);
                    var results = await _registrationService.RegisterClient(message, token);
                    switch (results.ResponseCode)
                    {
                        case ResponseCode.Ok:
                            break;
                        case ResponseCode.Rejected:
                            throw new RegistrationException(
                                "Progressive registration rejected to communicate to the server",
                                RegistrationFailureReason.Rejected);
                        default:
                            throw new RegistrationException(
                                "Progressive registration failed to communicate to the server",
                                RegistrationFailureReason.NoResponse);
                    }
                }
            }
            catch (Exception e) when (!(e is RegistrationException))
            {
                throw new RegistrationException("Progressive registration failed to communicate to the server", e, RegistrationFailureReason.NoResponse);
            }
        }
    }
}