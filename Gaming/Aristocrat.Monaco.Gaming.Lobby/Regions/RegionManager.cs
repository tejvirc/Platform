﻿namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using XAMLMarkupExtensions.Base;

public class RegionManager : DependencyObject, IRegionManager
{
    private readonly Dictionary<string, IRegion> _regions = new();

    /// <summary>
    ///     <see cref="DependencyProperty"/> set the target region.
    /// </summary>
    public static readonly DependencyProperty RegionProperty =
        DependencyProperty.RegisterAttached(
            "Region",
            typeof(string),
            typeof(RegionManager),
            new FrameworkPropertyMetadata(default(string),
                FrameworkPropertyMetadataOptions.None, OnRegionNameChanged));

    private static void OnRegionNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ContentControl contentControl)
        {
            contentControl.Loaded += (sender, args) => { this.CreateRegion(); };

            var adapter = _regionAdapterMapper.GetAdapter(contentControl.GetType());
            adapter.Adapt(e.NewValue, contentControl);
        }
    }

    /// <summary>
    ///     Gets or sets the <see cref="RegionProperty"/> value.
    /// </summary>
    public string Region
    {
        get => (string)GetValue(RegionProperty);

        set => SetValue(RegionProperty, value);
    }

    /// <summary>
    ///     Getter of <see cref="RegionProperty"/>.
    /// </summary>
    /// <param name="obj">The target dependency object.</param>
    /// <returns>The value of the property.</returns>
    public static string GetRegion(DependencyObject obj)
    {
        return obj?.GetValueSync<string>(RegionProperty);
    }

    /// <summary>
    ///     Setter of <see cref="RegionProperty"/>.
    /// </summary>
    /// <param name="obj">The target dependency object.</param>
    /// <param name="value">The value of the property.</param>
    public static void SetRegion(DependencyObject obj, string value)
    {
        obj?.SetValueSync(RegionProperty, value);
    }

    public async Task RegisterViewAsync<T>(string regionName, string viewName)
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

        await region.AddViewAsync(viewName);
    }

    public async Task<bool> NavigateViewAsync(string regionName, string viewName)
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

        return await region.NavigateToAsync(viewName);
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
