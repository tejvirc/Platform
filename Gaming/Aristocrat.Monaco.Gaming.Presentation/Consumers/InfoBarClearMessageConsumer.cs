namespace Aristocrat.Monaco.Gaming.Presentation.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Aristocrat.Monaco.Gaming.Contracts.InfoBar;
using Extensions.Fluxor;
using Fluxor;
using Store;

public class InfoBarClearMessageConsumer : Consumes<InfoBarClearMessageEvent>
{
    private readonly IDispatcher _dispatcher;

    public InfoBarClearMessageConsumer(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override async Task ConsumeAsync(InfoBarClearMessageEvent theEvent, CancellationToken cancellationToken)
    {
        await _dispatcher.DispatchAsync(new InfoBarClearMessageAction(theEvent.OwnerId, theEvent.DisplayTarget, theEvent.Regions));
    }
}
