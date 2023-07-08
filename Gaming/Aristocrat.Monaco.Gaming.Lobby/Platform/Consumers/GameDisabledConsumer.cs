namespace Aristocrat.Monaco.Gaming.Lobby.Platform.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Fluxor;
using Store;

public class GameDisabledConsumer : Consumes<GameDisabledEvent>
{
    private readonly IDispatcher _dispatcher;

    public GameDisabledConsumer(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override async Task ConsumeAsync(GameDisabledEvent theEvent, CancellationToken cancellationToken)
    {
        await _dispatcher.DispatchAsync(new GameDisabledAction());
    }
}
