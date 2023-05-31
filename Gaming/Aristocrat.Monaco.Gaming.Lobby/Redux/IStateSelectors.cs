namespace Aristocrat.Monaco.Gaming.Lobby.Redux;

public interface IStateSelectors<out TSelectors>
{
    TSelectors Selectors { get; }
}
