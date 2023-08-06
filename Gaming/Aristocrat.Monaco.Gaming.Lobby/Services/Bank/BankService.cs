namespace Aristocrat.Monaco.Gaming.Lobby.Services.Bank;

using Accounting.Contracts;
using UI.Utils;

public class BankService : IBankService
{
    private readonly IBank _bank;

    public BankService(IBank bank)
    {
        _bank = bank;
    }

    public double GetBalance() => OverlayMessageUtils.ToCredits(_bank.QueryBalance());
}
