namespace Aristocrat.Monaco.Gaming.Lobby.Consumers;

using System.Threading;
using System.Threading.Tasks;
using CommandHandlers;
using Kernel.Contracts.Events;
using Store;

public class InitializationCompletedConsumer : Consumes<InitializationCompletedEvent>
{
    private readonly ICommandHandlerFactory _commandHandlers;

    public InitializationCompletedConsumer(ICommandHandlerFactory commandHandlers)
    {
        _commandHandlers = commandHandlers;
    }

    public override Task ConsumeAsync(InitializationCompletedEvent theEvent, CancellationToken cancellationToken)
    {
        _commandHandlers.Create<InitializationCompleted>().Handle(new InitializationCompleted());

        return Task.CompletedTask;
    }
}
