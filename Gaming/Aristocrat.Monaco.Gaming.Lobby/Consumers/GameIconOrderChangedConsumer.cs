namespace Aristocrat.Monaco.Gaming.Lobby.Consumers;

using System.Threading;
using System.Threading.Tasks;
using CommandHandlers;
using Contracts;
using Store;

public class GameIconOrderChangedConsumer : Consumes<GameIconOrderChangedEvent>
{
    private readonly ICommandHandlerFactory _commandHandlers;

    public GameIconOrderChangedConsumer(ICommandHandlerFactory commandHandlers)
    {
        _commandHandlers = commandHandlers;
    }

    public override Task ConsumeAsync(GameIconOrderChangedEvent theEvent, CancellationToken cancellationToken)
    {
        _commandHandlers.Create<GameIconOrderChanged>().Handle(new GameIconOrderChanged());

        return Task.CompletedTask;
    }
}
