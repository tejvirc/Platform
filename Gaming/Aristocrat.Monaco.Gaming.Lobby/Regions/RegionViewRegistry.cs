namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

public class RegionViewRegistry : IRegionViewRegistry
{
    private readonly Dictionary<string, List<RegionViewRegistration>> _registrations = new();

    public void RegisterViewWithRegion<TView>(string regionName, string viewName) where TView : class
    {
        if (!_registrations.ContainsKey(regionName))
        {
            _registrations.Add(regionName, new List<RegionViewRegistration>());
        }

        var viewType = typeof(TView);

        var registration = new RegionViewRegistration
        {
            RegionName = regionName,
            ViewName = viewName,
            ViewCreator = () => Application.Current.GetService(viewType)
        };

        var index = _registrations[regionName].IndexOf(registration);
        if (index >= 0)
        {
            _registrations[regionName][index] = registration;
        }
        else
        {
            _registrations[regionName].Add(registration);
        }
    }

    public (string Name, object View)[] GetViews(string regionName)
    {
        if (!_registrations.TryGetValue(regionName, out var registrations))
        {
            return Array.Empty<(string, object)>();
        }

        return registrations.Select(v => (v.ViewName, v.ViewCreator.Invoke()))
            .ToArray();
    }
}
