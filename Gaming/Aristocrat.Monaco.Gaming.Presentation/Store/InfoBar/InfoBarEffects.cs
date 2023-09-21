namespace Aristocrat.Monaco.Gaming.Presentation.Store.InfoBar;

using System;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.Extensions.Logging;
using Services.InfoBar;

public class InfoBarEffects
{
    private readonly IState<InfoBarState> _infoBarState;
    private readonly IInfoBarService _infoBarService;
    private readonly ILogger<InfoBarEffects> _infoBarEffectsLogger;
    public InfoBarEffects(IState<InfoBarState> infoBarState, IInfoBarService infoBarService, ILogger<InfoBarEffects> infoBarEffectsLogger)
    {
        _infoBarState = infoBarState;
        _infoBarService = infoBarService;
        _infoBarEffectsLogger = infoBarEffectsLogger;
    }

    [EffectMethod]
    public Task Effect(InfoBarClearMessageAction action, IDispatcher dispatcher)
    {
        var tcs = new TaskCompletionSource<bool>();
        try
        {
            foreach (var region in action.Regions)
            {
                _infoBarService.ClearMessage(action.OwnerId, region);
            }

            if (string.IsNullOrWhiteSpace(_infoBarState.Value.LeftRegionText) &&
                string.IsNullOrWhiteSpace(_infoBarState.Value.CenterRegionText) &&
                string.IsNullOrWhiteSpace(_infoBarState.Value.RightRegionText))
            {
                dispatcher.Dispatch(new InfoBarCloseAction(_infoBarState.Value.DisplayTarget));
            }
        }
        catch (Exception ex)
        {
            _infoBarEffectsLogger.Log(LogLevel.Debug, $"Error clearing message owner={action.OwnerId} region={action.Regions}.", ex);
        }

        return tcs.Task;
    }
}
