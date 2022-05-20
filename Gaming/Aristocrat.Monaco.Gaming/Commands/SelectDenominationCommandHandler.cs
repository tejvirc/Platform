namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Models;
    using Kernel;
    using log4net;
    using Runtime;
    using Runtime.Client;

    /// <summary>
    ///     Command handler for the <see cref="SelectDenomination" /> command.
    /// </summary>
    public class SelectDenominationCommandHandler : ICommandHandler<SelectDenomination>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPropertiesManager _properties;
        private readonly IGameService _gameService;
        private readonly IRuntime _runtime;
        private readonly IEventBus _eventBus;
        private readonly IGameProvider _gameProvider;

        public SelectDenominationCommandHandler(
            IPropertiesManager properties,
            IGameService gameService,
            IRuntime runtime,
            IEventBus eventBus,
            IGameProvider gameProvider)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        /// <inheritdoc />
        public void Handle(SelectDenomination command)
        {
            var (currentGame, currentDenom) = _properties.GetActiveGame();

            var selectedDenom = command.Denomination.CentsToMillicents();

            if (currentDenom.Id == selectedDenom)
            {
                Logger.Debug($"In-game denom selection ignored.  The active denom is {currentDenom.Id}");
                return;
            }

            // While the theme will be the same the underlying Id may change based on the denomination
            var selectedGame = _gameProvider.GetEnabledGames().FirstOrDefault(
                g => g.ThemeId == currentGame.ThemeId && g.ActiveDenominations.Contains(selectedDenom));

            if (selectedGame == null)
            {
                Logger.Warn($"There are no enabled games for the selected denom: {currentGame.ThemeId} - {selectedDenom}");
                return;
            }

            var betOption = selectedGame.Denominations.First(d => d.Value == selectedDenom).BetOption;
            _eventBus.Publish(new DenominationSelectedEvent(selectedGame.Id, selectedDenom));

            _gameService.ReInitialize(
                new GameInitRequest
                {
                    GameId = selectedGame.Id,
                    Denomination = selectedDenom,
                    BetOption = betOption
                });

            _runtime.UpdateState(RuntimeState.Reconfigure);

            Logger.Debug($"In-game denom selection changed: {selectedGame.Id} - {selectedDenom}");
        }
    }
}