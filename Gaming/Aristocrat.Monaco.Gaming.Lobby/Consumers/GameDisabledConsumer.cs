namespace Aristocrat.Monaco.Gaming.Lobby.Consumers;

using System.Threading;
using System.Threading.Tasks;
using CommandHandlers;
using Contracts;
using Store;

public class GameDisabledConsumer : Consumes<GameDisabledEvent>
{
    private readonly ICommandHandlerFactory _commandHandlers;

    public GameDisabledConsumer(ICommandHandlerFactory commandHandlers)
    {
        _commandHandlers = commandHandlers;
    }

    public override Task ConsumeAsync(GameDisabledEvent theEvent, CancellationToken cancellationToken)
    {
        _commandHandlers.Create<GameDisabled>().Handle(new GameDisabled());

        return Task.CompletedTask;
    }
}
