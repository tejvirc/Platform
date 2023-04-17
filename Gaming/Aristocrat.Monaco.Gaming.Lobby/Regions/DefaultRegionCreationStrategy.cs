namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System;
using System.Threading.Tasks;
using System.Windows;

public class DefaultRegionCreationStrategy : RegionCreationStrategy
{
    private readonly IRegionManager _regionManager;
    private readonly IRegionNavigator _regionNavigator;

    public DefaultRegionCreationStrategy(IRegionManager regionManager, IRegionNavigator regionNavigator)
    {
        _regionManager = regionManager;
        _regionNavigator = regionNavigator;
    }

    public Task<IRegion> CreateRegionAsync(DependencyObject element)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        var regionName = GetRegionName(element);

        var region = new Region(_regionManager, _regionNavigator)
        {
            Name = regionName
        };

        return Task.FromResult<IRegion>(region);
    }
}
