namespace Aristocrat.Monaco.Gaming.Presentation.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Aristocrat.Monaco.Gaming.Contracts.InfoBar;
using Extensions.Fluxor;
using Fluxor;
using Store;

public class InfoBarCloseConsumer : Consumes<InfoBarCloseEvent>
{
    private readonly IDispatcher _dispatcher;

    public InfoBarCloseConsumer(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override async Task ConsumeAsync(InfoBarCloseEvent theEvent, CancellationToken cancellationToken)
    {
        await _dispatcher.DispatchAsync(new InfoBarCloseAction(theEvent.DisplayTarget));
    }
}

