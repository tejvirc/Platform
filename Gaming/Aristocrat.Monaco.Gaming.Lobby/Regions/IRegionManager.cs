namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System.Collections.Generic;

public interface IRegionManager : IEnumerable<IRegion>
{
    void RegisterView<TView>(string regionName, string viewName) where TView : class;
}
