namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System.Windows;

public interface IRegionCreator<TStrategy> where TStrategy : IRegionCreationStrategy
{
    void Create(FrameworkElement element);
}
