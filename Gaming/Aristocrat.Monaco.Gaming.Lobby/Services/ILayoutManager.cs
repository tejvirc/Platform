namespace Aristocrat.Monaco.Gaming.Lobby.Services;

using System.Threading.Tasks;

public interface ILayoutManager
{
    Task InitializeAsync();

    void DestroyWindows();
}
