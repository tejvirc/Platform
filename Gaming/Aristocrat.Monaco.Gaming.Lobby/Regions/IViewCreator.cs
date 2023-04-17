namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

public interface IViewCreator
{
    
}

public interface IViewCreator<TStrategy> : IViewCreator
    where TStrategy : IViewCreator
{

}
