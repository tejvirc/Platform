namespace Aristocrat.Monaco.Gaming.Lobby.Redux;

public class StateSelectors<TSelectors> : IStateSelectors<TSelectors>
{
    public StateSelectors(TSelectors selectors)
    {
        Selectors = selectors;
    }

    public TSelectors Selectors { get; }
}
