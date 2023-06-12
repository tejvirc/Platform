namespace Aristocrat.Monaco.Gaming.Lobby.Store.Kernel;

public record KernelState
{
    public bool IsSystemDisabled { get; set; }

    public bool IsSystemDisableImmediately { get; set; }
}
