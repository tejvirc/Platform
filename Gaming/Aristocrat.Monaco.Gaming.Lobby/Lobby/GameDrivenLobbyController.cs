namespace Aristocrat.Monaco.Gaming.Lobby.Lobby;

using System.Threading;
using System.Threading.Tasks;

public class GameDrivenLobbyController : LobbyController
{
    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task GameAddedAsync(int gameId, string themeId)
    {
        return Task.CompletedTask;
    }

    public Task GameDenomChangedAsync(int gameId, string denoms)
    {
        return Task.CompletedTask;
    }
}
