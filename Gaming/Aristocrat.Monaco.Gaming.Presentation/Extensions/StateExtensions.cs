namespace Aristocrat.Monaco.Gaming.Presentation;

using Store.Banner;
using Store.Bank;

public static class StateExtensions
{
    public static bool HasZeroCredits(this BankState state) => state.Credits.Equals(0.0);

    public static bool IsTextScrolling(this BannerState state) => state.IsScrolling;
}
