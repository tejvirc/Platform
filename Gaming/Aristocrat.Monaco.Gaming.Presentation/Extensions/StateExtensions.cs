namespace Aristocrat.Monaco.Gaming.Presentation;

using Contracts.Lobby;
using Store.IdleText;
using Store.Bank;

public static class StateExtensions
{
    public static bool HasZeroCredits(this BankState state) => state.Credits.Equals(0.0);

    public static bool IsTextScrolling(this IdleTextState state) => state.BannerDisplayMode == BannerDisplayMode.Scrolling;
}
