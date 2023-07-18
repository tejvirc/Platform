namespace Aristocrat.Monaco.Gaming.Lobby.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using UI.Models;

public interface IGameLoader
{
    Task<IEnumerable<GameInfo>> LoadGames();
}
