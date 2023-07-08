namespace Aristocrat.Monaco.Gaming.Lobby.Store.Attract;

using Extensions.Fluxor;
using static Extensions.Fluxor.Selectors;

public static class AttractSelectors
{
    public static readonly ISelector<AttractState, bool> IsAlternateTopImageActiveSelector = CreateSelector(
        (AttractState s) => s.IsAlternateTopImageActive);

    public static readonly ISelector<AttractState, bool> IsAlternateTopperImageActiveSelector = CreateSelector(
        (AttractState s) => s.IsAlternateTopperImageActive);

    public static readonly ISelector<AttractState, int> AttractModeTopImageIndexSelector = CreateSelector(
        (AttractState s) => s.AttractModeTopImageIndex);

    public static readonly ISelector<AttractState, int> AttractModeTopperImageIndexSelector = CreateSelector(
        (AttractState s) => s.AttractModeTopperImageIndex);
}
