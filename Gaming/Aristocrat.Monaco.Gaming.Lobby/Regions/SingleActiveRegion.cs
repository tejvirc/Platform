namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System.Linq;

public class SingleActiveRegion : Region
{
    public SingleActiveRegion(IRegionManager regionManager, IRegionNavigator regionNavigator, string regionName)
        : base(regionManager, regionNavigator, regionName)
    {
    }

    public override void ActivateView(object view)
    {
        var currentActiveView = ActiveViews.FirstOrDefault();
        if (currentActiveView != null && currentActiveView != view && Views.Contains(currentActiveView))
        {
            DeactivateView(currentActiveView);
        }

        base.ActivateView(view);
    }
}
