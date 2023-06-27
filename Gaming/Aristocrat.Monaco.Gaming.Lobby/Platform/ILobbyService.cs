namespace Aristocrat.Monaco.Gaming.Lobby.Runtime;

using Aristocrat.LobbyRuntime.V1;

public interface ILobbyService
{
    GetGamesResponse GetGames(Empty request);
}
