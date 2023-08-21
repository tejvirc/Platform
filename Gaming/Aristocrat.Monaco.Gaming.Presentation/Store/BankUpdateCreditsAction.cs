namespace Aristocrat.Monaco.Gaming.Presentation.Store;

public record BankUpdateCreditsAction
{
    public BankUpdateCreditsAction(double credits)
    {
        Credits = credits;
    }

    public double Credits { get; }
}
