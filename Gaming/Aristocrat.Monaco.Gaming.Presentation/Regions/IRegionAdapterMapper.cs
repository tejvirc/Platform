namespace Aristocrat.Monaco.Gaming.Presentation.Regions;

using System;

public interface IRegionAdapterMapper
{
    IRegionAdapter GetAdapter(Type elementType);
}
