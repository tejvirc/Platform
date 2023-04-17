namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System.Collections.Generic;
using System.Threading.Tasks;

public interface IRegionManager : IEnumerable<IRegion>
{
    Task RegisterViewAsync<T>(string regionName, string viewName);
}
