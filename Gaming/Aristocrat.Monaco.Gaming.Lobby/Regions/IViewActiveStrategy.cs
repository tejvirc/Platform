namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System.Threading.Tasks;

public interface IViewActiveStrategy
{
    Task<bool> ActivateViewAsync(IRegion region, object view);

    Task<bool> DeactivateViewAsync(IRegion region, object view);
}
