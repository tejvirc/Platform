namespace Aristocrat.Monaco.Gaming.Lobby.Consumers;

using System.Threading;
using System.Threading.Tasks;
using CommandHandlers;
using Contracts;
using Store;

public class GameUninstalledConsumer : Consumes<GameUninstalledEvent>
{
    private readonly ICommandHandlerFactory _commandHandlers;

    public GameUninstalledConsumer(ICommandHandlerFactory commandHandlers)
    {
        _commandHandlers = commandHandlers;
    }

    public override Task ConsumeAsync(GameUninstalledEvent theEvent, CancellationToken cancellationToken)
    {
        _commandHandlers.Create<GameUninstalled>().Handle(new GameUninstalled());

        return Task.CompletedTask;
    }
}
