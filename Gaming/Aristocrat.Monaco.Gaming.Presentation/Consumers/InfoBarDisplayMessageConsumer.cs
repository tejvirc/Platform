namespace Aristocrat.Monaco.Gaming.Presentation.Consumers;
using System.Threading;
using System.Threading.Tasks;
using Aristocrat.Monaco.Gaming.Presentation.Store.InfoBar;
using Extensions.Fluxor;
using Fluxor;
using Gaming.Contracts.InfoBar;
using Store;

public class InfoBarDisplayMessageConsumer : Consumes<InfoBarDisplayTransientMessageEvent>
{
    private readonly IDispatcher _dispatcher;
    private readonly IState<InfoBarState> _infoBarState;

    public InfoBarDisplayMessageConsumer(IDispatcher dispatcher, IState<InfoBarState> infoBarState)
    {
        _dispatcher = dispatcher;
        _infoBarState = infoBarState;
    }

    public override async Task ConsumeAsync(InfoBarDisplayTransientMessageEvent theEvent, CancellationToken cancellationToken)
    {
        if (theEvent.DisplayTarget == _infoBarState.Value.DisplayTarget)
        {
            await _dispatcher.DispatchAsync(
                new InfoBarDisplayMessageAction(theEvent.OwnerId,
                    theEvent.Message,
                    theEvent.Duration,
                    theEvent.TextColor,
                    theEvent.BackgroundColor,
                    theEvent.Region,
                    theEvent.DisplayTarget));
        }
    }
}