namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System.Threading.Tasks;
using System.Windows;

public interface IRegionCreator
{
    Task<IRegion> CreateAsync(DependencyObject control);
}

public interface IRegionCreator<TStrategy> : IRegionCreator
    where TStrategy : IRegionCreationStrategy
{
}
