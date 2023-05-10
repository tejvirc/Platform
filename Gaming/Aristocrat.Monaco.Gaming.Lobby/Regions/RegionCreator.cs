namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System.Windows;

public class RegionCreator<TStrategy> : IRegionCreator<TStrategy> where TStrategy : IRegionCreationStrategy
{
    private readonly TStrategy _strategy;

    public RegionCreator(TStrategy strategy)
    {
        _strategy = strategy;
    }

    public void Create(FrameworkElement element)
    {
        _strategy.CreateRegion(element);
    }
}
