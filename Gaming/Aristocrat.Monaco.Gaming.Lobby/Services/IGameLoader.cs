namespace Aristocrat.Monaco.Gaming.Lobby.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Models;

public interface IGameLoader
{
    Task<IEnumerable<GameInfo>> LoadGames();
}