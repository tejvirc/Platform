namespace Aristocrat.Monaco.Gaming.Lobby.Store;

public class SelectGameAction
{
    public SelectGameAction(int gameId, long denom)
    {
        GameId = gameId;
        Denom = denom;
    }

    public int GameId { get; }

    public long Denom { get; }
}
