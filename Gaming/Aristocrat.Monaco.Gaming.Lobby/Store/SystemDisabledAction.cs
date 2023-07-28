namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using System;
using System.Collections.Generic;
using System.Linq;
using Kernel;

public record SystemDisabledAction
{
    public SystemDisabledAction(
        SystemDisablePriority priority,
        bool isSystemDisabled,
        bool isSystemDisableImmediately,
        IEnumerable<Guid> disableKeys,
        IEnumerable<Guid> immediateDisableKeys)
    {
        Priority = priority;
        IsSystemDisabled = isSystemDisabled;
        IsSystemDisableImmediately = isSystemDisableImmediately;
        DisableKeys = disableKeys.ToList();
        ImmediateDisableKeys = immediateDisableKeys.ToList();
    }

    public SystemDisablePriority Priority { get; }

    public bool IsSystemDisabled { get; }

    public bool IsSystemDisableImmediately { get; }

    public IReadOnlyList<Guid> DisableKeys { get; }

    public IReadOnlyList<Guid> ImmediateDisableKeys { get; }
}
