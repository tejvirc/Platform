namespace Aristocrat.Monaco.Gaming.Presentation.Regions;

using System;
using System.Windows;

public class DelayedRegionCreationStrategy : IRegionCreationStrategy
{
    private readonly IRegionManager _regionManager;
    private readonly IRegionAdapterMapper _regionAdapterMapper;
    private readonly IRegionViewRegistry _regionViewRegistry;

    public DelayedRegionCreationStrategy(IRegionManager regionManager, IRegionAdapterMapper regionAdapterMapper, IRegionViewRegistry regionViewRegistry)
    {
        _regionManager = regionManager;
        _regionAdapterMapper = regionAdapterMapper;
        _regionViewRegistry = regionViewRegistry;
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

        var region = regionAdapter.CreateRegion(regionName, element);

        PopulateViews(region);

        _regionManager.AddRegion(region);
    }

    private void PopulateViews(IRegion region)
    {
        var views = _regionViewRegistry.GetViews(region.Name);

        foreach (var (name, view) in views)
        {
            region.AddView(name, view);
        }
    }

    private static string? GetRegionName(DependencyObject element) =>
        element.GetValue(RegionManager.RegionNameProperty) as string;
}
