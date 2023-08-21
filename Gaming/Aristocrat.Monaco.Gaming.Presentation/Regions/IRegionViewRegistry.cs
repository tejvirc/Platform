namespace Aristocrat.Monaco.Gaming.Presentation.Regions;

public interface IRegionViewRegistry
{
    void RegisterViewWithRegion<TView>(string regionName, string viewName) where TView : class;

    (string Name, object View)[] GetViews(string regionName);
}
