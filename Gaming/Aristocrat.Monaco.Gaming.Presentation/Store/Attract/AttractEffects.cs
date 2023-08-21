namespace Aristocrat.Monaco.Gaming.Presentation.Store.Attract;

using System.Threading.Tasks;
using Chooser;
using Extensions.Fluxor;
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

    [EffectMethod(typeof(AttractEnterAction))]
    public async Task Enter(IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new AttractEnteredAction());
    }

    [EffectMethod(typeof(AttractEnteredAction))]
    public Task Entered(IDispatcher dispatcher)
    {
        _attractService.NotifyEntered();

        return Task.CompletedTask;
    }

    [EffectMethod(typeof(AttractExitAction))]
    public async Task Exit(IDispatcher dispatcher)
    {
        var currentAttractIndex = _attractState.Value.CurrentAttractIndex + 1;

        if (currentAttractIndex >= _attractState.Value.Videos.Count)
        {
            currentAttractIndex = 0;
        }

        await dispatcher.DispatchAsync(new AttractUpdateIndexAction { AttractIndex = currentAttractIndex });

        var nextAttractModeLanguageIsPrimary = _attractState.Value.IsNextLanguagePrimary;
        var lastInitialAttractModeLanguageIsPrimary = _attractState.Value.IsLastInitialLanguagePrimary;

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
            new TranslateUpdatePrimaryLanguageIndicators
            {
                NextAttractModeLanguageIsPrimary = nextAttractModeLanguageIsPrimary,
                LastInitialAttractModeLanguageIsPrimary = lastInitialAttractModeLanguageIsPrimary
            });

        _attractService.SetAttractVideoPaths(currentAttractIndex);

        _attractService.RotateTopImage();

        _attractService.RotateTopperImage();

        await dispatcher.DispatchAsync(new AttractUpdateConsecutiveCount { ConsecutiveAttractCount = 0 });

        await dispatcher.DispatchAsync(new AttractExitedAction());
    }

    [EffectMethod(typeof(AttractVideoCompletedAction))]
    public async Task VideoCompleted(IDispatcher dispatcher)
    {
        var consecutiveAttractCount = _attractState.Value.ConsecutiveAttractCount;

        if (!_configuration.HasAttractIntroVideo || _attractState.Value.CurrentAttractIndex != 0 || _attractState.Value.Videos.Count <= 1)
        {
            consecutiveAttractCount++;

            await dispatcher.DispatchAsync(new AttractUpdateConsecutiveCount { ConsecutiveAttractCount = consecutiveAttractCount });

            if (consecutiveAttractCount >= _configuration.ConsecutiveAttractVideos ||
                consecutiveAttractCount >= _chooserState.Value.Games.Count)
            {
                await dispatcher.DispatchAsync(new AttractExitAction());
                return;
            }
        }

        await dispatcher.DispatchAsync(new AttractVideoNextAction());
    }

    [EffectMethod(typeof(GameUninstalledAction))]
    public async Task GameUninstalled(IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new AttractUpdateIndexAction { AttractIndex = 0 });

        _attractService.SetAttractVideoPaths(_attractState.Value.CurrentAttractIndex);
    }

    [EffectMethod(typeof(GameLoadedAction))]
    public async Task GameLoaded(IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new AttractUpdateIndexAction { AttractIndex = 0 });

        _attractService.SetAttractVideoPaths(_attractState.Value.CurrentAttractIndex);
    }
}
