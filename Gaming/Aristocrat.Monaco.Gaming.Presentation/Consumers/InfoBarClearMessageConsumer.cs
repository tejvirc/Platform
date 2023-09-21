namespace Aristocrat.Monaco.Gaming.Presentation.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Aristocrat.Monaco.Gaming.Contracts.InfoBar;
using Aristocrat.Monaco.Gaming.Presentation.Store.InfoBar;
using Extensions.Fluxor;
using Fluxor;
using Store;

public class InfoBarClearMessageConsumer : Consumes<InfoBarClearMessageEvent>
{
    private readonly IDispatcher _dispatcher;
    private readonly IState<InfoBarState> _infoBarState;

    public InfoBarClearMessageConsumer(IDispatcher dispatcher, IState<InfoBarState> infoBarState)
    {
        _dispatcher = dispatcher;
        _infoBarState = infoBarState;
    }

    public override async Task ConsumeAsync(InfoBarClearMessageEvent theEvent, CancellationToken cancellationToken)
    {
        if (theEvent.DisplayTarget == _infoBarState.Value.DisplayTarget)
        {
            await _dispatcher.DispatchAsync(
                new InfoBarClearMessageAction(theEvent.OwnerId, theEvent.DisplayTarget, theEvent.Regions));
        }
    }
}
