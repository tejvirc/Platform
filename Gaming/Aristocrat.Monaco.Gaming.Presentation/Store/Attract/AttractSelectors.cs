namespace Aristocrat.Monaco.Gaming.Presentation.Store.Attract;

using Extensions.Fluxor;
using static Extensions.Fluxor.Selectors;

public static class AttractSelectors
{
    public static readonly ISelector<AttractState, bool> SelectIsAlternateTopImageActive = CreateSelector(
        (AttractState s) => s.IsAlternateTopImageActive);

    public static readonly ISelector<AttractState, bool> SelectIsAlternateTopperImageActive = CreateSelector(
        (AttractState s) => s.IsAlternateTopperImageActive);

    public static readonly ISelector<AttractState, int> SelectAttractModeTopImageIndex = CreateSelector(
        (AttractState s) => s.ModeTopImageIndex);

    public static readonly ISelector<AttractState, int> SelectAttractModeTopperImageIndex = CreateSelector(
        (AttractState s) => s.ModeTopperImageIndex);

    public static readonly ISelector<AttractState, bool> SelectAttractStarting = CreateSelector(
        (AttractState s) => s.IsActive);
}
