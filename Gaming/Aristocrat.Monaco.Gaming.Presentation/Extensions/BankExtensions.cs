namespace Aristocrat.Monaco.Gaming.Presentation;

using Store.Bank;

public static class BankExtensions
{
    public static bool HasZeroCredits(this BankState state) =>
        state.Credits.Equals(0.0);
}
