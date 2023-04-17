namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System.Windows.Controls;

public class ContentControlRegionAdapter : RegionAdapter<ContentControl>
{
    public ContentControlRegionAdapter(
        IRegionCreator<DefaultRegionCreationStrategy> regionCreator,
        IViewActivator<SingleViewActiveStrategy> viewActivator)
        : base(regionCreator, viewActivator)
    {
    }
}
