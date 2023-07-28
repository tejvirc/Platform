namespace Aristocrat.Monaco.Gaming.Lobby.Consumers;

using System.Threading;
using System.Threading.Tasks;
using CommandHandlers;
using Contracts;
using Store;

public class GameEnabledConsumer : Consumes<GameEnabledEvent>
{
    private readonly ICommandHandlerFactory _commandHandlers;

    public GameEnabledConsumer(ICommandHandlerFactory commandHandlers)
    {
        _commandHandlers = commandHandlers;
    }

    public override Task ConsumeAsync(GameEnabledEvent theEvent, CancellationToken cancellationToken)
    {
        _commandHandlers.Create<GameEnabled>().Handle(new GameEnabled());

        return Task.CompletedTask;
    }
}
