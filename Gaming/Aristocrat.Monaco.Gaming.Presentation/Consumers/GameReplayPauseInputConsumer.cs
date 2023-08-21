namespace Aristocrat.Monaco.Gaming.Presentation.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Extensions.Fluxor;
using Fluxor;
using Store;

public class GameReplayPauseInputConsumer : Consumes<GameReplayPauseInputEvent>
{
    private readonly IDispatcher _dispatcher;

    public GameReplayPauseInputConsumer(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override async Task ConsumeAsync(GameReplayPauseInputEvent theEvent, CancellationToken cancellationToken)
    {
        await _dispatcher.DispatchAsync(new ReplayPauseInputAction(theEvent.ReplayPauseState));
    }
}
