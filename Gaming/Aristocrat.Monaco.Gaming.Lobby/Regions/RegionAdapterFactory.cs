namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class RegionAdapterFactory : IRegionAdapterFactory
{
    private readonly IServiceProvider _services;
    private readonly Dictionary<Type, IRegionAdapter> _adapters;

    public RegionAdapterFactory(IServiceProvider services)
    {
        _services = services;
    }

    public IRegionAdapter Create(Type controlType)
    {
        var adapters = 
        var adapters = adapters.ToDictionary(x => x.ControlType, x => x);
        var lookupType = controlType;

        while (lookupType != null)
        {
            if (_adapters.TryGetValue(lookupType, out var adapter))
            {
                return adapter;
            }

            lookupType = lookupType.BaseType;
        }

        throw new ArgumentOutOfRangeException(nameof(controlType));
    }
}
