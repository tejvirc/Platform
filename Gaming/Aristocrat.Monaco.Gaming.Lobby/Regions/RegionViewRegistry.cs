namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System;
using System.Collections.Generic;
using System.Linq;

public class RegionViewRegistry : IRegionViewRegistry
{
    private readonly Dictionary<string, List<RegionViewRegistration>> _registrations = new();

    public void RegisterViewWithRegion<TView>(string regionName, string viewName) where TView : class
    {
        if (!_registrations.ContainsKey(regionName))
        {
            _registrations.Add(regionName, new List<RegionViewRegistration>());
        }

        var registration = new RegionViewRegistration { RegionName = regionName, ViewName = viewName, ViewCreator = () => Activator.CreateInstance(typeof(TView)) };

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

    public IDictionary<string, object?> GetViews(string regionName)
    {
        if (!_registrations.TryGetValue(regionName, out var registrations))
        {
            throw new ArgumentOutOfRangeException(nameof(regionName));
        }

        return registrations.ToDictionary(v => v.ViewName, v => v.ViewCreator.Invoke());
    }
}
