namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System.Threading.Tasks;

public class MultiViewsActiveStrategy : IViewActiveStrategy
{
    public Task<bool> ActivateViewAsync(IRegion region, object view)
    {
        return Task.FromResult(false);
    }

    public Task<bool> DeactivateViewAsync(IRegion region, object view)
    {
        return Task.FromResult(false);
    }
}
