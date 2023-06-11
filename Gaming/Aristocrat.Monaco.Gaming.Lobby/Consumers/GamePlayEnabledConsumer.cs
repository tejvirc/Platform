namespace Aristocrat.Monaco.Gaming.Lobby.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Fluxor;
using Store;

public class GamePlayEnabledConsumer : Consumes<GamePlayEnabledEvent>
{
    private readonly IDispatcher _dispatcher;

    public GamePlayEnabledConsumer(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override Task ConsumeAsync(GamePlayEnabledEvent theEvent, CancellationToken cancellationToken)
    {
        _dispatcher.Dispatch(new GamePlayEnabledAction());

        return Task.CompletedTask;
    }
}
