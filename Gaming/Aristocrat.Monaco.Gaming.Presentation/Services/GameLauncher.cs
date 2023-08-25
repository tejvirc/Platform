namespace Aristocrat.Monaco.Gaming.Presentation.Services;

using System;
using Contracts;
using Fluxor;
using Kernel;
using Microsoft.Extensions.Logging;
using Store.Game;
using UI.Models;

public sealed class GameLauncher : IGameLauncher
{
    private readonly ILogger<GameLauncher> _logger;
    private readonly IState<GameState> _gameState;
    private readonly IEventBus _eventBus;
    private readonly IGameDiagnostics _gameDiagnostics;

    public GameLauncher(
        ILogger<GameLauncher> logger,
        IState<GameState> gameState,
        IEventBus eventBus,
        IGameDiagnostics gameDiagnostics)
    {
        _logger = logger;
        _gameState = gameState;
        _eventBus = eventBus;
        _gameDiagnostics = gameDiagnostics;
    }

    public void LaunchGame(GameInfo game)
    {
        _eventBus.Publish(
            new GameSelectedEvent(
                game.GameId,
                game.Denomination,
                game.BetOption,
                _gameDiagnostics.IsActive,
                _gameState.Value.MainWindowHandle,
                _gameState.Value.TopWindowHandle,
                _gameState.Value.ButtonDeckWindowHandle,
                _gameState.Value.TopperWindowHandle));
    }
}
