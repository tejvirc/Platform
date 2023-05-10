namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System;
using System.Windows;

public class DelayedRegionCreationStrategy : IRegionCreationStrategy
{
    private readonly IRegionAdapterMapper _regionAdapterMapper;

    public DelayedRegionCreationStrategy(IRegionAdapterMapper regionAdapterMapper)
    {
        _regionAdapterMapper = regionAdapterMapper;
    }

    public void CreateRegion(FrameworkElement element)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        element.Loaded += OnElementLoaded;
    }

    private void OnElementLoaded(object sender, RoutedEventArgs args)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        element.Loaded -= OnElementLoaded;

        var regionName = GetRegionName(element);
        if (regionName == null)
        {
            return;
        }

        CreateRegion(regionName, element);
    }

    private void CreateRegion(string regionName, FrameworkElement element)
    {
        var regionAdapter = _regionAdapterMapper.GetAdapter(element.GetType());

        regionAdapter.CreateRegion(regionName, element);
    }

    private static string? GetRegionName(DependencyObject element) =>
        element.GetValue(RegionManager.RegionNameProperty) as string;
}
