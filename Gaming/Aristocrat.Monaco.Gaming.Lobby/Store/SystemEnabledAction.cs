namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using System;
using System.Collections.Generic;
using System.Linq;

public record SystemEnabledAction
{
    public SystemEnabledAction(
        bool isSystemDisabled,
        bool isSystemDisableImmediately,
        IEnumerable<Guid> disableKeys,
        IEnumerable<Guid> immediateDisableKeys)
    {
        IsSystemDisabled = isSystemDisabled;
        IsSystemDisableImmediately = isSystemDisableImmediately;
        DisableKeys = disableKeys.ToList();
        ImmediateDisableKeys = immediateDisableKeys.ToList();
    }

    public bool IsSystemDisabled { get; }

    public bool IsSystemDisableImmediately { get; }

    public IReadOnlyList<Guid> DisableKeys { get; }

    public IReadOnlyList<Guid> ImmediateDisableKeys { get; }
}
