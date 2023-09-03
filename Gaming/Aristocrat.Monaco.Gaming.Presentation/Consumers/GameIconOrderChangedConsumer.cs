namespace Aristocrat.Monaco.Gaming.Presentation.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Extensions.Fluxor;
using Fluxor;
using Gaming.Contracts;
using Store;

public class GameIconOrderChangedConsumer : Consumes<GameIconOrderChangedEvent>
{
    private readonly IDispatcher _dispatcher;

    public GameIconOrderChangedConsumer(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override async Task ConsumeAsync(GameIconOrderChangedEvent theEvent, CancellationToken cancellationToken)
    {
        await _dispatcher.DispatchAsync(new GameListIconOrderChangedAction());
    }
}
