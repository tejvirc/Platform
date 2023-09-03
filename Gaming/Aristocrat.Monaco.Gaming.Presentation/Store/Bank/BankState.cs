namespace Aristocrat.Monaco.Gaming.Presentation.Store.Bank;

using Gaming.Contracts.Models;

public record BankState
{
    public bool CashOutEnabled { get; init; }

    public double Credits { get; init; }

    public bool IsCashingOut { get; init; }

    public LobbyCashOutState CurrentCashOutState { get; init; }
}
