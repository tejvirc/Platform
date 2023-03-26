namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using Aristocrat.Monaco.Kernel;

public class SystemDisabledAction
{
    public SystemDisabledAction(SystemDisablePriority priority)
    {
        Priority = priority;
    }

    /// <summary>
    ///     Gets the priority of the disable.
    /// </summary>
    public SystemDisablePriority Priority { get; }
}
