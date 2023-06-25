namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using System.Collections.Immutable;
using Models;
using Fluxor.Extensions;
using static Fluxor.Extensions.Selectors;

public static class LobbySelectors
{
    public static readonly ISelector<LobbyState, IImmutableList<GameInfo>> GamesSelector = CreateSelector(
        (LobbyState s) => s.Games);

    public static readonly ISelector<LobbyState, string?> IdleTextSelector = CreateSelector(
        (LobbyState s) => s.IdleText);

    public static readonly ISelector<LobbyState, bool> IsAlternateTopImageActiveSelector = CreateSelector(
        (LobbyState s) => s.IsAlternateTopImageActive);

    public static readonly ISelector<LobbyState, bool> IsAlternateTopperImageActiveSelector = CreateSelector(
        (LobbyState s) => s.IsAlternateTopperImageActive);

    public static readonly ISelector<LobbyState, int> AttractModeTopImageIndexSelector = CreateSelector(
        (LobbyState s) => s.AttractModeTopperImageIndex);

    public static readonly ISelector<LobbyState, int> AttractModeTopperImageIndexSelector = CreateSelector(AttractModeTopImageIndexSelector,
        (int x) => x);
}
