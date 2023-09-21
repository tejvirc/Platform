namespace Aristocrat.Monaco.Gaming.Presentation.Store.Attract;

using System.Threading.Tasks;
using Aristocrat.Monaco.Accounting;
using Aristocrat.Monaco.Accounting.Contracts;
using Aristocrat.Monaco.Gaming.Presentation.Options;
using Aristocrat.Monaco.Kernel;
using Chooser;
using Extensions.Fluxor;
using Fluxor;
using Microsoft.Extensions.Options;
using Services.Attract;
using Services.EdgeLighting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

public partial class AttractEffects
{
    private readonly IState<AttractState> _attractState;
    private readonly IState<ChooserState> _chooserState;
    private readonly IAttractService _attractService;
    private readonly ITopImageRotationService _topImageRotationService;
    private readonly ITopperImageRotationService _topperImageRotationService;
    private readonly AttractOptions _attractOptions;
    private readonly ResponsibleGamingOptions _responsibleGamingOptions;
    private readonly IEdgeLightingService _edgeLightingService;
    private readonly IPropertiesManager _properties;
    private readonly IBank _bank;

    public AttractEffects(
        IState<AttractState> attractState,
        IState<ChooserState> chooserState,
        IAttractService attractService,
        ITopImageRotationService topImageRotationService,
        ITopperImageRotationService topperImageRotationService,
        IOptions<AttractOptions> attractOptions,
        IOptions<ResponsibleGamingOptions> responsibleGamingOptions,
        IPropertiesManager properties,
        IBank bank,
        IEdgeLightingService edgeLightingService)
    {
        _attractState = attractState;
        _chooserState = chooserState;
        _attractService = attractService;
        _topImageRotationService = topImageRotationService;
        _topperImageRotationService = topperImageRotationService;
        _attractOptions = attractOptions.Value;
        _responsibleGamingOptions = responsibleGamingOptions.Value;
        _edgeLightingService = edgeLightingService;
        _properties = properties;
        _bank = bank;
    }

    [EffectMethod(typeof(StartupAction))]
    public async Task Startup(IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new AttractSetCanModeStartAction { Bank = _bank, Properties = _properties });
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

        if (_attractOptions.AlternateLanguage)
        {
            nextAttractModeLanguageIsPrimary = !nextAttractModeLanguageIsPrimary;
        }

        if (currentAttractIndex == 0 && _attractOptions.AlternateLanguage)
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

        _topImageRotationService.RotateTopImage();

        _topperImageRotationService.RotateTopperImage();

        await dispatcher.DispatchAsync(new AttractUpdateConsecutiveCount { ConsecutiveAttractCount = 0 });

        await dispatcher.DispatchAsync(new AttractExitedAction());
    }

    [EffectMethod(typeof(AttractExitedAction))]
    public Task Exited(IDispatcher dispatcher)
    {
        _attractService.NotifyExited();

        return Task.CompletedTask;
    }

    [EffectMethod(typeof(AttractVideoCompletedAction))]
    public async Task VideoCompleted(IDispatcher dispatcher)
    {
        var consecutiveAttractCount = _attractState.Value.ConsecutiveAttractCount;

        if (!_attractOptions.HasIntroVideo || _attractState.Value.CurrentAttractIndex != 0 || _attractState.Value.Videos.Count <= 1)
        {
            consecutiveAttractCount++;

            await dispatcher.DispatchAsync(new AttractUpdateConsecutiveCount { ConsecutiveAttractCount = consecutiveAttractCount });

            if (consecutiveAttractCount >= _attractOptions.ConsecutiveVideos ||
                consecutiveAttractCount >= _chooserState.Value.Games.Count)
            {
                _topImageRotationService.RotateTopImage();
                _topperImageRotationService.RotateTopperImage();
                await dispatcher.DispatchAsync(new AttractExitAction());
                return;
            }
        }

        await dispatcher.DispatchAsync(new AttractVideoNextAction());
    }

    [EffectMethod(typeof(AttractVideoNextAction))]
    public async Task NextVideo(IDispatcher dispatcher)
    {
        if (_attractState.Value.Videos.Count <= 1)
        {
            await dispatcher.DispatchAsync(new AttractExitAction());
        }

        await dispatcher.DispatchAsync(new AttractUpdateIndexAction { AttractIndex = _attractService.AdvanceAttractIndex() });
        _attractService.SetAttractVideoPaths(_attractState.Value.CurrentAttractIndex);
    }

    [EffectMethod(typeof(GameUninstalledAction))]
    public async Task GameUninstalled(IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new AttractUpdateIndexAction { AttractIndex = 0 });

        _attractService.RefreshAttractGameList();
    }

    [EffectMethod(typeof(GameLoadedAction))]
    public async Task GameLoaded(IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new AttractUpdateIndexAction { AttractIndex = 0 });

        _attractService.RefreshAttractGameList();
    }

    [EffectMethod(typeof(GameListLoadedAction))]
    public async Task GameListLoaded(IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new AttractUpdateIndexAction { AttractIndex = 0 });

        _attractService.RefreshAttractGameList();
    }

    [EffectMethod(typeof(UserInteractedAction))]
    public async Task UserInteracted(IDispatcher dispatcher)
    {
        //TODO REVIEW WHY THIS IS IN HERE TWICE
        (_attractService as AttractService)?.CheckAndStartAttractTimer();

        // Don't display Age Warning while the inserting cash dialog is up.
        // TODO (Future Task) When we have a set way to determine if the current state matches with the old LobbyState.CashIn
        // we can add an && check for that after the config.DisplayAgeWarning check to make it match directly with the old functionality.
        if (!_responsibleGamingOptions.AgeWarningEnabled && _attractState.Value.IsPlaying)
        {
            await dispatcher.DispatchAsync(new AttractExitAction());
        }

        if (_attractState.Value.ResetAttractOnInterruption && _attractState.Value.CurrentAttractIndex != 0)
        {
            await dispatcher.DispatchAsync(new AttractUpdateIndexAction { AttractIndex = 0 });
            _attractService.SetAttractVideoPaths(_attractState.Value.CurrentAttractIndex);
        }
    }
}
