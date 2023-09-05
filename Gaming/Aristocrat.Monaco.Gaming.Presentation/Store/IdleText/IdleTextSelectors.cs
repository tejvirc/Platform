namespace Aristocrat.Monaco.Gaming.Presentation.Store.IdleText;

using Extensions.Fluxor;
using static Extensions.Fluxor.Selectors;

public static class IdleTextSelectors
{
    public static readonly ISelector<IdleTextState, string?> SelectIdleText = CreateSelector(
        (IdleTextState s) => s.IdleText);
}
