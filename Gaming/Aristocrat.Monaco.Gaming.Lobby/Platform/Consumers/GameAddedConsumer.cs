namespace Aristocrat.Monaco.Gaming.Lobby.Platform.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Fluxor;
using Store;

public class GameAddedConsumer : Consumes<GameAddedEvent>
{
    private readonly IDispatcher _dispatcher;
    private readonly IGameOrderSettings _gameOrderSettings;

    public GameAddedConsumer(IDispatcher dispatcher, IGameOrderSettings gameOrderSettings)
    {
        _dispatcher = dispatcher;
        _gameOrderSettings = gameOrderSettings;
    }

    public override async Task ConsumeAsync(GameAddedEvent theEvent, CancellationToken cancellationToken)
    {
        _gameOrderSettings.OnGameAdded(theEvent.ThemeId);

        await _dispatcher.DispatchAsync(new GameIconOrderChangedAction());
    }
}
