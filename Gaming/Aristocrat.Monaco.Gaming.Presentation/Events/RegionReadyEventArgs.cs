namespace Aristocrat.Monaco.Gaming.Presentation.Events;

using System.Windows;
using Prism.Regions;

public class RegionReadyEventArgs : RoutedEventArgs
{
    public RegionReadyEventArgs(RoutedEvent routedEvent, object source, IRegion region)
        : base(routedEvent, source)
    {
        Region = region;
    }

    public IRegion Region { get; }
}

public delegate void RegionReadyEventHandler(object sender, RegionReadyEventArgs e);
