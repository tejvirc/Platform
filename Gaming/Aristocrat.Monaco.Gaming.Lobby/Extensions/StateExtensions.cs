namespace Aristocrat.Monaco.Gaming.Lobby;

using Contracts.Lobby;
using Store.IdleText;
using Store.Lobby;

public static class StateExtensions
{
    public static bool HasZeroCredits(this LobbyState state) => state.Equals(0.0);

    public static bool IsTextScrolling(this IdleTextState state) => state.BannerDisplayMode == BannerDisplayMode.Scrolling;
}
