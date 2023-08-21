namespace Aristocrat.Monaco.Gaming.Presentation.Regions;

using System.Collections.Generic;

public interface IRegionManager : IEnumerable<IRegion>
{
    void AddRegion(IRegion region);

    void RegisterView<TView>(string regionName, string viewName) where TView : class;

    bool NavigateToView(string regionName, string viewName);
}
