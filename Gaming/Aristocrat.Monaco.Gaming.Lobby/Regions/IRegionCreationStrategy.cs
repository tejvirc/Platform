namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System.Threading.Tasks;
using System.Windows;

public interface IRegionCreationStrategy
{
    Task<IRegion> CreateRegionAsync(DependencyObject element);
}
