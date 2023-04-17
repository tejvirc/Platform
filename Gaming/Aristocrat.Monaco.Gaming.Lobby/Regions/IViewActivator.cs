namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System.Threading.Tasks;

public interface IViewActivator
{
    Task<bool> ActivateAsync(IRegion region, object view);

    Task<bool> DeactivateAsync(IRegion region, object view);
}

public interface IViewActivator<TStrategy> : IViewActivator
    where TStrategy : IViewActiveStrategy
{
}
