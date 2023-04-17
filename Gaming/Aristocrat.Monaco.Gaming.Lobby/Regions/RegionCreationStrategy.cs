namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System.Windows;

public class RegionCreationStrategy : IRegionCreationStrategy
{
    protected static string GetRegionName(DependencyObject element)
    {
        return element.GetValue(RegionManager.RegionProperty) as string;
    }
}
