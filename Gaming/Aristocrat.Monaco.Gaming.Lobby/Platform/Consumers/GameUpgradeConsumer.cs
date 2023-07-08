namespace Aristocrat.Monaco.Gaming.Lobby.Platform.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Fluxor;
using Store;

public class GameUpgradeConsumer : Consumes<GameUpgradedEvent>
{
    private readonly IDispatcher _dispatcher;

    public GameUpgradeConsumer(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override async Task ConsumeAsync(GameUpgradedEvent theEvent, CancellationToken cancellationToken)
    {
        await _dispatcher.DispatchAsync(new GameUpgradedAction());
    }
}
