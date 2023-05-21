namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System;
using System.Windows;

public abstract class RegionAdapter<TElement> : IRegionAdapter<TElement>
    where TElement : FrameworkElement
{
    public Type ControlType => typeof(TElement);

    public IRegion CreateRegion(string regionName, FrameworkElement element)
    {
        if (element is not TElement targetElement)
        {
            throw new ArgumentException($"{element.GetType()} is an invalid element type");
        }

        var region = NewRegion(regionName);

        Adapt(region, targetElement);

        return region;
    }

    protected abstract IRegion NewRegion(string regionName);

    protected abstract void Adapt(IRegion region, TElement element);
}
