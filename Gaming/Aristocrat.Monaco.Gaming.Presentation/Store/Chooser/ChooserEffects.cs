namespace Aristocrat.Monaco.Gaming.Presentation.Store.Chooser;

using System.Threading.Tasks;
using Aristocrat.Monaco.Gaming.Presentation.Services.Chooser;
using Contracts.Audio;
using Extensions.Fluxor;
using Fluxor;
using Microsoft.Extensions.Logging;
using Services;
using Services.Audio;
using Store.Game;

public class ChooserEffects
{
    private readonly ILogger<ChooserEffects> _logger;
    private readonly IState<ChooserState> _chooserState;
    private readonly IState<GameState> _gameState;
    private readonly IAudioService _audioService;
    private readonly IGameLauncher _gameLauncher;
    private readonly IGameFilterService _gameFilterService;

    public ChooserEffects(
        ILogger<ChooserEffects> logger,
        IState<ChooserState> chooserState,
        IState<GameState> gameState,
        IAudioService audioService,
        IGameLauncher gameLauncher,
        IGameFilterService gameFilterService)
    {
        _logger = logger;
        _chooserState = chooserState;
        _gameState = gameState;
        _audioService = audioService;
        _gameLauncher = gameLauncher;
        _gameFilterService = gameFilterService;
    }

    [EffectMethod]
    public async Task GameSelected(ChooserGameSelectedAction action, IDispatcher dispatcher)
    {
        await _audioService.PlaySoundAsync(SoundType.First);

        if (_gameState.Value.IsLoaded || _gameState.Value.IsLoading)
        {
            _logger.LogDebug("Rejecting Game Launch because runtime process has not yet exited.");
            return;
        }

        _gameLauncher.LaunchGame(action.Game);

        await dispatcher.DispatchAsync(new GameLoadingAction(action.Game));
    }

    [EffectMethod(typeof(UserInteractionAction))]
    public Task UserInteraction(IDispatcher _)
    {
        _gameFilterService.ResetIdleTimer();

        return Task.CompletedTask;
    }
}
