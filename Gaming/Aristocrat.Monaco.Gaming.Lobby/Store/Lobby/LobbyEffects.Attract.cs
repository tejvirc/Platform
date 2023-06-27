namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using Fluxor;
using System.Threading.Tasks;
using Contracts.Lobby;
using Controllers.Attract;
using Controllers.EdgeLighting;
using Aristocrat.Monaco.Gaming.Lobby.Services.EdgeLighting;
using Aristocrat.Monaco.Gaming.Lobby.Services.Attract;

public partial class LobbyEffectsAttract
{
    private readonly IState<LobbyState> _state;
    private readonly IAttractService _attractService;
    private readonly IEdgeLightingService _edgeLightingService;

    public LobbyEffectsAttract(
        IState<LobbyState> state,
        IAttractService attractService,
        IEdgeLightingService edgeLightingService)
    {
        _state = state;
        _attractService = attractService;
        _edgeLightingService = edgeLightingService;
    }

    [EffectMethod]
    public async Task Effect(AttractEnterAction _, IDispatcher dispatcher)
    {
        _edgeLightingService.SetEdgeLightOverride();

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
        var currentAttractIndex = _attractService.AdvanceAttractIndex();

        await dispatcher.DispatchAsync(new UpdateAttractIndexAction { AttractIndex = currentAttractIndex });

        _attractService.SetLanguageFlags(currentAttractIndex);

        _edgeLightingService.SetEdgeLighting();

        var attractVideoPaths = _attractService.SetAttractVideoPaths(currentAttractIndex);

        await dispatcher.DispatchAsync(
            new UpdateAttractVideosAction
            {
                TopAttractVideoPath = attractVideoPaths.TopAttractVideoPath,
                TopperAttractVideoPath = attractVideoPaths.TopperAttractVideoPath,
                BottomAttractVideoPath = attractVideoPaths.BottomAttractVideoPath
            });

        _attractService.ResetConsecutiveAttractCount();

        await dispatcher.DispatchAsync(new AttractExitedAction());
    }

    [EffectMethod]
    public async Task Effect(AttractVideoCompletedAction _, IDispatcher dispatcher)
    {
        if (_attractService.PlayAdditionalConsecutiveVideo())
        {
            await dispatcher.DispatchAsync(new AttractVideoNextAction { });
        }
        else
        {
            await dispatcher.DispatchAsync(new AttractExitAction { });
        }
    }
}
