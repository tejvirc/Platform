namespace Aristocrat.Monaco.Gaming.Lobby.Store.Upi;

using Aristocrat.Extensions.Fluxor;
using static Extensions.Fluxor.Selectors;

public static class UpiSelectors
{
    public static readonly ISelector<UpiState, bool> SelectServiceAvailable = CreateSelector(
        (UpiState state) => state.IsServiceAvailable);

    public static readonly ISelector<UpiState, bool> SelectServiceEnabled = CreateSelector(
        (UpiState state) => state.IsServiceEnabled);

    public static readonly ISelector<UpiState, bool> SelectVolumeControlEnabled = CreateSelector(
        (UpiState state) => state.IsVolumeControlEnabled);
}
