namespace Aristocrat.Monaco.Gaming.Presentation.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using UI.Models;

public interface IGameLoader
{
    Task<IEnumerable<GameInfo>> LoadGames();
}
