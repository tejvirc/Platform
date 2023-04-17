namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System;
using System.Windows.Controls;

public class RegionAdapter<TControl> : IRegionAdapter<TControl>
    where TControl : Control
{
    public RegionAdapter(IRegionCreator regionCreator, IViewActivator viewActivator)
    {
        
    }

    public Type ControlType => typeof(TControl);
}
