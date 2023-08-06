namespace Aristocrat.Monaco.Gaming.Lobby.Store;

public record UpdateDenomFilterAction
{
    public UpdateDenomFilterAction(int denomFilter)
    {
        DenomFilter = denomFilter;
    }

    public int DenomFilter { get; }
}
