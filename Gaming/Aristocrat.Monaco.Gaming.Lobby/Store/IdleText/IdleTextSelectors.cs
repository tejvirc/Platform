namespace Aristocrat.Monaco.Gaming.Lobby.Store.IdleText;

using Extensions.Fluxor;
using static Extensions.Fluxor.Selectors;

public static class IdleTextSelectors
{
    public static readonly ISelector<IdleTextState, string?> IdleTextSelector = CreateSelector(
        (IdleTextState s) => s.IdleText);
}
