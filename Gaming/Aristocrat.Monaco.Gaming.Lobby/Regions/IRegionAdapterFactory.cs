namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System;

public interface IRegionAdapterFactory
{
    IRegionAdapter Create(Type controlType);
}
