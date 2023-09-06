namespace Aristocrat.Monaco.Gaming.Presentation.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Gaming.Contracts;
using Extensions.Fluxor;
using Fluxor;
using Store;

public class GameUpgradedConsumer : Consumes<GameUpgradedEvent>
{
    private readonly IDispatcher _dispatcher;

    public GameUpgradedConsumer(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override async Task ConsumeAsync(GameUpgradedEvent theEvent, CancellationToken cancellationToken)
    {
        await _dispatcher.DispatchAsync(new GameUpgradedAction());
    }
}
