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
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly IGameService _gameService;
        private readonly IRuntime _runtime;
        private readonly IEventBus _eventBus;
        private readonly IGameProvider _gameProvider;

        public SelectDenominationCommandHandler(
            IGameService gameService,
            IRuntime runtime,
            IEventBus eventBus,
            IGameProvider gameProvider)
        {
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        /// <inheritdoc />
        public void Handle(SelectDenomination command)
        {
            var (currentGame, currentDenom) = _gameProvider.GetActiveGame();

            var selectedDenomValue = command.Denomination.CentsToMillicents();

            // While the theme will be the same the underlying Id may change based on the denomination
            var selectedGame = _gameProvider.GetEnabledGames().FirstOrDefault(
                g => g.ThemeId == currentGame.ThemeId && g.ActiveDenominations.Contains(selectedDenomValue));

            if (selectedGame == null)
            {
                Logger.Warn($"There are no enabled games for the selected denom: {currentGame.ThemeId} - {selectedDenomValue}");
                return;
            }

            // Even if we haven't technically "changed" denomination, we still need to publish this
            // event so we can report coming out of the "game selection screen".
            _eventBus.Publish(new DenominationSelectedEvent(selectedGame.Id, selectedDenomValue));

            var selectedDenom = selectedGame.Denominations.Single(d => d.Value == selectedDenomValue);

            if (currentDenom.Id == selectedDenom.Id)
            {
                Logger.Debug($"In-game denom selection ignored. The active denom is {currentDenom.Id}");
                return;
            }

            var betOption = selectedDenom.BetOption;

            _gameService.ReInitialize(
                new GameInitRequest
                {
                    GameId = selectedGame.Id,
                    Denomination = selectedDenomValue,
                    BetOption = betOption
                });

            _runtime.UpdateState(RuntimeState.Reconfigure);

            Logger.Debug($"In-game denom selection changed: {selectedGame.Id} - {selectedDenomValue}");
        }
    }
}