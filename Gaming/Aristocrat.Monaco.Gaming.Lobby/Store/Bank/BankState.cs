namespace Aristocrat.Monaco.Gaming.Lobby.Store.Bank;

public record BankState
{
    public bool CashOutEnabled { get; init; }

    public double Credits { get; init; }
}
