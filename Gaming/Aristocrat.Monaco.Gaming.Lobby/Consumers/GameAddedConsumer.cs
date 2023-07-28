namespace Aristocrat.Monaco.Gaming.Lobby.Consumers;

using System.Threading;
using System.Threading.Tasks;
using CommandHandlers;
using Contracts;
using Store;

public class GameAddedConsumer : Consumes<GameAddedEvent>
{
    private readonly ICommandHandlerFactory _commandHandlers;
    private readonly IGameOrderSettings _gameOrderSettings;

    public GameAddedConsumer(ICommandHandlerFactory commandHandlers, IGameOrderSettings gameOrderSettings)
    {
        _commandHandlers = commandHandlers;
        _gameOrderSettings = gameOrderSettings;
    }

    public override Task ConsumeAsync(GameAddedEvent theEvent, CancellationToken cancellationToken)
    {
        _gameOrderSettings.OnGameAdded(theEvent.ThemeId);

        _commandHandlers.Create<GameAdded>().Handle(new GameAdded());

        return Task.CompletedTask;
    }
}
