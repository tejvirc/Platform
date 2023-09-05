namespace Aristocrat.Monaco.Gaming.Presentation;

using Gaming.Contracts.Lobby;
using Store.Bank;
using Store.IdleText;

public static class StateExtensions
{
    public static bool HasZeroCredits(this BankState state) => state.Credits.Equals(0.0);

    public static bool IsTextScrolling(this IdleTextState state) => state.BannerDisplayMode == BannerDisplayMode.Scrolling;
}
