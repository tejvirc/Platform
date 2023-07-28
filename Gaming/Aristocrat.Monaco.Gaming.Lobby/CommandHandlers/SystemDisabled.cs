namespace Aristocrat.Monaco.Gaming.Lobby.CommandHandlers;

using System;
using System.Collections.Generic;
using System.Linq;
using Kernel;

public class SystemDisabled
{
    public SystemDisabled(
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
