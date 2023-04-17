namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System.Threading.Tasks;

public interface IRegionNavigator
{
    Task<bool> NavigateToAsync(string viewName);
}
