namespace Aristocrat.Monaco.Gaming.Presentation.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Extensions.Fluxor;
using Fluxor;
using Gaming.Contracts;
using Store;

public class GameEnabledConsumer : Consumes<GameEnabledEvent>
{
    private readonly IDispatcher _dispatcher;

    public GameEnabledConsumer(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override async Task ConsumeAsync(GameEnabledEvent theEvent, CancellationToken cancellationToken)
    {
        await _dispatcher.DispatchAsync(new GameEnabledAction());
    }
}
