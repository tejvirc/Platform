namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System;
using System.Collections.Generic;
using SimpleInjector;

public class RegionAdapterMapper : IRegionAdapterMapper
{
    private readonly Container _container;
    private readonly Dictionary<Type, InstanceProducer<IRegionAdapter>> _producers = new();

    public RegionAdapterMapper(Container container)
    {
        _container = container;
    }

    public IRegionAdapter GetAdapter(Type elementType)
    {
        var currentType = elementType;

        while (currentType != null)
        {
            if (_producers.TryGetValue(currentType, out var handler))
            {
                return handler.GetInstance();
            }

            currentType = currentType.BaseType;
        }

        throw new InvalidOperationException($"No adapter found for {elementType} type");
    }

    internal void Register<TImplementation>(Type elementType)
        where TImplementation : class, IRegionAdapter
    {
        var producer = Lifestyle.Transient
            .CreateProducer<IRegionAdapter, TImplementation>(_container);

        _producers.Add(elementType, producer);
    }
}
