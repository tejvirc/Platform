namespace Aristocrat.Monaco.Gaming.Lobby.Store;

public record UpdateCreditsAction
{
    public UpdateCreditsAction(double credits)
    {
        Credits = credits;
    }

    public double Credits { get; }
}
