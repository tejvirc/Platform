namespace Aristocrat.Monaco.Gaming.Lobby.Consumers;

using System.Threading;
using System.Threading.Tasks;
using CommandHandlers;
using Contracts;
using Store;

public class GameUpgradedConsumer : Consumes<GameUpgradedEvent>
{
    private readonly ICommandHandlerFactory _commandHandlers;

    public GameUpgradedConsumer(ICommandHandlerFactory commandHandlers)
    {
        _commandHandlers = commandHandlers;
    }

    public override Task ConsumeAsync(GameUpgradedEvent theEvent, CancellationToken cancellationToken)
    {
        _commandHandlers.Create<GameUpgraded>().Handle(new GameUpgraded());

        return Task.CompletedTask;
    }
}
