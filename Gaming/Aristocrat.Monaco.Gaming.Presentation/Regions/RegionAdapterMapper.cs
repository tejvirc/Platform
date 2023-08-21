namespace Aristocrat.Monaco.Gaming.Presentation.Regions;

using System;

public delegate IRegionAdapter CreateRegionAdapter(Type elementType);

public class RegionAdapterMapper : IRegionAdapterMapper
{
    private readonly CreateRegionAdapter _factory;

    public RegionAdapterMapper(CreateRegionAdapter factory)
    {
        _factory = factory;
    }

    /// <inheritdoc />
    public IRegionAdapter GetAdapter(Type elementType)
    {
        return _factory.Invoke(elementType);
    }
}
