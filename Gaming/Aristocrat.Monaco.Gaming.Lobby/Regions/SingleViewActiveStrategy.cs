namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

public class SingleViewActiveStrategy : IViewActiveStrategy
{
    public bool ActivateView(IRegion region, object view)
    {
        return false;
    }

    public bool DeactivateView(IRegion region, object view)
    {
        return false;
    }
}
