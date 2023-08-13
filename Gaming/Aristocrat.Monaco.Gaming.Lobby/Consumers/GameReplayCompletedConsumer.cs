namespace Aristocrat.Monaco.Gaming.Lobby.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Fluxor;

internal class GameReplayCompletedConsumer : Consumes<GameReplayCompletedEvent>
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
