namespace Aristocrat.Monaco.Gaming.Presentation.Regions;

using System;
using System.Windows;
using System.Windows.Controls;

public interface IRegionAdapter
{
    Type ControlType { get; }

    IRegion CreateRegion(string regionName, FrameworkElement element);
}

public interface IRegionAdapter<TElement> : IRegionAdapter
    where TElement : FrameworkElement
{
}
