namespace Aristocrat.Monaco.Gaming.Lobby.Services;

using System.Collections.Generic;
using Models;

public interface IGameLoader
{
    IEnumerable<GameInfo> LoadGames();
}