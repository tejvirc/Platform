namespace Aristocrat.Monaco.Gaming.Lobby.Store;

public record UpdateServiceAvailable
{
    public UpdateServiceAvailable(bool isAvaiable)
    {
        IsAvaiable = isAvaiable;
    }

    public bool IsAvaiable { get; }
}
