﻿namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows;
using Aristocrat.MVVM;
using Extensions.Prism;
using XAMLMarkupExtensions.Base;

public class RegionManager : DependencyObject, IRegionManager
{
    private readonly IRegionViewRegistry _regionViewRegistry;

    private readonly Dictionary<string, IRegion> _regions = new();

    public RegionManager(IRegionViewRegistry regionViewRegistry)
    {
        _regionViewRegistry = regionViewRegistry;
    }

    /// <summary>
    ///     <see cref="DependencyProperty"/> set the target region.
    /// </summary>
    public static readonly DependencyProperty RegionNameProperty =
        DependencyProperty.RegisterAttached(
            "RegionName",
            typeof(string),
            typeof(RegionManager),
            new PropertyMetadata(default(string), OnRegionNameChanged));

    private static void OnRegionNameChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        if (obj is not FrameworkElement element)
        {
            return;
        }

        if (!(bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue)
        {
            CreateRegion(element);
        }
    }

    private static void CreateRegion(FrameworkElement element)
    {
        var creator = Application.Current.GetService<IRegionCreator<DelayedRegionCreationStrategy>>();
        creator.Create(element);
    }

    /// <summary>
    ///     Gets or sets the <see cref="RegionNameProperty"/> value.
    /// </summary>
    public string RegionName
    {
        get => (string)GetValue(RegionNameProperty);

        set => SetValue(RegionNameProperty, value);
    }

    /// <summary>
    ///     Getter of <see cref="RegionNameProperty"/>.
    /// </summary>
    /// <param name="obj">The target dependency object.</param>
    /// <returns>The value of the property.</returns>
    public static string? GetRegionName(DependencyObject obj)
    {
        return obj.GetValueSync<string>(RegionNameProperty);
    }

    /// <summary>
    ///     Setter of <see cref="RegionNameProperty"/>.
    /// </summary>
    /// <param name="obj">The target dependency object.</param>
    /// <param name="value">The value of the property.</param>
    public static void SetRegionName(DependencyObject obj, string value)
    {
        obj.SetValueSync(RegionNameProperty, value);
    }

    public void AddRegion(IRegion region)
    {
        _regions.Add(region.Name, region);
    }

    public void RegisterView<TView>(string regionName, string viewName)
        where TView : class
    {
        if (string.IsNullOrWhiteSpace(regionName))
        {
            throw new ArgumentNullException(nameof(regionName));
        }

        if (string.IsNullOrWhiteSpace(viewName))
        {
            throw new ArgumentNullException(nameof(viewName));
        }

        _regionViewRegistry.RegisterViewWithRegion<TView>(regionName, viewName);
    }

    public bool NavigateToView(string regionName, string viewName)
    {
        if (string.IsNullOrWhiteSpace(regionName))
        {
            throw new ArgumentNullException(nameof(regionName));
        }

        if (string.IsNullOrWhiteSpace(viewName))
        {
            throw new ArgumentNullException(nameof(viewName));
        }

        if (!_regions.TryGetValue(regionName, out var region))
        {
            throw new ArgumentOutOfRangeException(nameof(regionName));
        }

        return region.NavigateTo(viewName);
    }

    public IEnumerator<IRegion> GetEnumerator()
    {
        return _regions.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
