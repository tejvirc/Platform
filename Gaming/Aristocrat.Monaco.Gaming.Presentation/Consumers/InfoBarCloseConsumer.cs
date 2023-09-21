namespace Aristocrat.Monaco.Gaming.Presentation.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Extensions.Fluxor;
using Fluxor;
using Gaming.Contracts.InfoBar;
using Store;
using Store.InfoBar;

public class InfoBarCloseConsumer : Consumes<InfoBarCloseEvent>
{
    private readonly IDispatcher _dispatcher;
    private readonly IState<InfoBarState> _infoBarState;

    public InfoBarCloseConsumer(IDispatcher dispatcher, IState<InfoBarState> infoBarState)
    {
        _dispatcher = dispatcher;
        _infoBarState = infoBarState;
    }

    public override async Task ConsumeAsync(InfoBarCloseEvent theEvent, CancellationToken cancellationToken)
    {
        if (theEvent.DisplayTarget == _infoBarState.Value.DisplayTarget)
        {
            await _dispatcher.DispatchAsync(new InfoBarCloseAction(theEvent.DisplayTarget));
        }
    }
}