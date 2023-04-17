namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System.Threading.Tasks;
using System.Windows;

public class RegionCreator<TStrategy> : IRegionCreator<TStrategy>
    where TStrategy : IRegionCreationStrategy
{
    private readonly TStrategy _strategy;

    public RegionCreator(TStrategy strategy)
    {
        _strategy = strategy;
    }

    public async Task<IRegion> CreateAsync(DependencyObject control)
    {
        return await _strategy.CreateRegionAsync(control);
    }
}
