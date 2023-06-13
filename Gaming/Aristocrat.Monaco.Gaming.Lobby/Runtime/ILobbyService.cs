namespace Aristocrat.Monaco.Gaming.Lobby.Runtime;

using Aristocrat.Runtime.V1;

public interface ILobbyService
{
    GetGamesResponse GetGames(GetGamesRequest request);
}
