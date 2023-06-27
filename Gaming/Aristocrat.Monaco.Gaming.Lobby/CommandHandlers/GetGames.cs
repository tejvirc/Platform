namespace Aristocrat.Monaco.Gaming.Lobby.CommandHandlers;

using System.Collections.Generic;
using Aristocrat.LobbyRuntime.V1;

public class GetGames
{
    public IList<GameDetail>? Games { get; set; }
}
