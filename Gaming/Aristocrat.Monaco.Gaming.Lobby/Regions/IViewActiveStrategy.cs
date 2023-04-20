namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System.Threading.Tasks;

public interface IViewActiveStrategy
{
    bool ActivateView(IRegion region, object view);

    bool DeactivateView(IRegion region, object view);
}
