namespace Aristocrat.Monaco.Gaming.Lobby.Platform.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Fluxor;
using Store;

public class GameOrderChangedConsumer : Consumes<GameOrderChangedEvent>
{
    private readonly IDispatcher _dispatcher;

    public GameOrderChangedConsumer(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override async Task ConsumeAsync(GameOrderChangedEvent theEvent, CancellationToken cancellationToken)
    {
        await _dispatcher.DispatchAsync(new GameOrderChangedAction());
    }
}
