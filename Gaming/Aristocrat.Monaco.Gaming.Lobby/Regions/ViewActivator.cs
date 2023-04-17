namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System.Threading.Tasks;

public abstract class ViewActivator<TStrategy> : IViewActivator<TStrategy>
    where TStrategy : IViewActiveStrategy
{
    private readonly TStrategy _strategy;

    public ViewActivator(TStrategy strategy)
    {
        _strategy = strategy;
    }

    public async Task<bool> ActivateAsync(IRegion region, object view)
    {
        return await _strategy.ActivateViewAsync(region, view);
    }

    public async Task<bool> DeactivateAsync(IRegion region, object view)
    {
        return await _strategy.DeactivateViewAsync(region, view);
    }
}
