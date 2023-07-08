namespace Aristocrat.Monaco.Gaming.Lobby.Store.Attract;

using System.Threading.Tasks;
using Chooser;
using Fluxor;
using Services.Attract;
using Services.EdgeLighting;

public partial class AttractEffects
{
    private readonly IState<AttractState> _attractState;
    private readonly IState<ChooserState> _chooserState;
    private readonly IAttractService _attractService;
    private readonly LobbyConfiguration _configuration;
    private readonly IEdgeLightingService _edgeLightingService;

    public AttractEffects(
        IState<AttractState> attractState,
        IState<ChooserState> chooserState,
        IAttractService attractService,
        LobbyConfiguration configuration,
        IEdgeLightingService edgeLightingService)
    {
        _attractState = attractState;
        _chooserState = chooserState;
        _attractService = attractService;
        _configuration = configuration;
        _edgeLightingService = edgeLightingService;
    }

    [EffectMethod]
    public async Task Effect(AttractEnterAction _, IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new AttractEnteredAction());
    }

    [EffectMethod]
    public Task Effect(AttractEnteredAction _, IDispatcher dispatcher)
    {
        _attractService.NotifyEntered();

        return Task.CompletedTask;
    }

    [EffectMethod]
    public async Task Effect(AttractExitAction _, IDispatcher dispatcher)
    {
        var currentAttractIndex = _attractState.Value.CurrentAttractIndex + 1;

        if (currentAttractIndex >= _attractState.Value.AttractVideos.Count)
        {
            currentAttractIndex = 0;
        }

        await dispatcher.DispatchAsync(new UpdateAttractIndexAction { AttractIndex = currentAttractIndex });

        var nextAttractModeLanguageIsPrimary = _attractState.Value.NextAttractModeLanguageIsPrimary;
        var lastInitialAttractModeLanguageIsPrimary = _attractState.Value.LastInitialAttractModeLanguageIsPrimary;

        if (_configuration.AlternateAttractModeLanguage)
        {
            nextAttractModeLanguageIsPrimary = !nextAttractModeLanguageIsPrimary;
        }

        if (currentAttractIndex == 0 && _configuration.AlternateAttractModeLanguage)
        {
            nextAttractModeLanguageIsPrimary = !lastInitialAttractModeLanguageIsPrimary;
            lastInitialAttractModeLanguageIsPrimary = nextAttractModeLanguageIsPrimary;
        }

        await dispatcher.DispatchAsync(
            new UpdatePrimaryLanguageIndicators
            {
                NextAttractModeLanguageIsPrimary = nextAttractModeLanguageIsPrimary,
                LastInitialAttractModeLanguageIsPrimary = lastInitialAttractModeLanguageIsPrimary
            });

        _attractService.SetAttractVideoPaths(currentAttractIndex);

        _attractService.RotateTopImage();

        _attractService.RotateTopperImage();

        await dispatcher.DispatchAsync(new UpdateConsecutiveAttractCount { ConsecutiveAttractCount = 0 });

        await dispatcher.DispatchAsync(new AttractExitedAction());
    }

    [EffectMethod]
    public async Task Effect(AttractVideoCompletedAction _, IDispatcher dispatcher)
    {
        var consecutiveAttractCount = _attractState.Value.ConsecutiveAttractCount;

        if (!_configuration.HasAttractIntroVideo || _attractState.Value.CurrentAttractIndex != 0 || _attractState.Value.AttractVideos.Count <= 1)
        {
            consecutiveAttractCount++;

            await dispatcher.DispatchAsync(new UpdateConsecutiveAttractCount { ConsecutiveAttractCount = consecutiveAttractCount });

            if (consecutiveAttractCount >= _configuration.ConsecutiveAttractVideos ||
                consecutiveAttractCount >= _chooserState.Value.Games.Count)
            {
                await dispatcher.DispatchAsync(new AttractExitAction());
                return;
            }
        }

        await dispatcher.DispatchAsync(new AttractVideoNextAction());
    }

    [EffectMethod]
    public async Task Effect(GameUninstalledAction _, IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new UpdateAttractIndexAction { AttractIndex = 0 });

        _attractService.SetAttractVideoPaths(_attractState.Value.CurrentAttractIndex);
    }

    [EffectMethod]
    public async Task Effect(GameLoadedAction _, IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new UpdateAttractIndexAction { AttractIndex = 0 });

        _attractService.SetAttractVideoPaths(_attractState.Value.CurrentAttractIndex);
    }
}
