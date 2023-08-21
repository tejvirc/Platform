namespace Aristocrat.Monaco.Gaming.Presentation.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Extensions.Fluxor;
using Fluxor;
using Store;

public class GameReplayCompletedConsumer : Consumes<GameReplayCompletedEvent>
{
    private readonly IDispatcher _dispatcher;

    public GameReplayCompletedConsumer(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override async Task ConsumeAsync(GameReplayCompletedEvent theEvent, CancellationToken cancellationToken)
    {
        await _dispatcher.DispatchAsync(new ReplayCompletedAction());
    }
}
