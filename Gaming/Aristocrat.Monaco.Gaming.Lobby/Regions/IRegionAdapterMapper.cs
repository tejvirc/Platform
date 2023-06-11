namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System;

public interface IRegionAdapterMapper
{
    IRegionAdapter GetAdapter(Type elementType);
}
