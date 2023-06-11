namespace Aristocrat.Monaco.Gaming.Lobby.Store.Application;

public record ApplicationState
{
    public bool IsSystemDisabled { get; set; }

    public bool IsSystemDisableImmediately { get; set; }
}
