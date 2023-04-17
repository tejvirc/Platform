namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System;
using System.Windows.Controls;

public interface IRegionAdapter
{
    Type ControlType { get; }
}

public interface IRegionAdapter<TControl> : IRegionAdapter
    where TControl : Control
{
}
