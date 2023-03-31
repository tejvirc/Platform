namespace Aristocrat.Monaco.Gaming.Lobby.Store;

public class GameLaunchedAction
{
    public GameLaunchedAction(int gameId)
    {
        GameId = gameId;
    }

    public int GameId { get; }
}
