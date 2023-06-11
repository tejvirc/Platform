namespace Aristocrat.Monaco.Gaming.Lobby.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Fluxor;
using Store;

public class GamePlayDisabledConsumer : Consumes<GamePlayDisabledEvent>
{
    private readonly IDispatcher _dispatcher;

    public GamePlayDisabledConsumer(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override Task ConsumeAsync(GamePlayDisabledEvent theEvent, CancellationToken cancellationToken)
    {
        _dispatcher.Dispatch(new GamePlayDisabledAction());

        return Task.CompletedTask;
    }
}
