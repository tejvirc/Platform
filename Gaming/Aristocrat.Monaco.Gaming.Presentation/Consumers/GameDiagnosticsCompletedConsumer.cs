namespace Aristocrat.Monaco.Gaming.Presentation.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Gaming.Contracts;
using Extensions.Fluxor;
using Fluxor;
using Store;

public class GameDiagnosticsCompletedConsumer : Consumes<GameDiagnosticsCompletedEvent>
{
    private readonly IDispatcher _dispatcher;

    public GameDiagnosticsCompletedConsumer(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override async Task ConsumeAsync(GameDiagnosticsCompletedEvent theEvent, CancellationToken cancellationToken)
    {
        await _dispatcher.DispatchAsync(new ReplayCompletedAction());
    }
}
