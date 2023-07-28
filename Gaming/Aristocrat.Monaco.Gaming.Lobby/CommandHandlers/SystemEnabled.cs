namespace Aristocrat.Monaco.Gaming.Lobby.CommandHandlers;

using System;
using System.Collections.Generic;
using System.Linq;

public class SystemEnabled
{
    public SystemEnabled(
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

    public bool IsSystemDisabled { get; init; }

    public bool IsSystemDisableImmediately { get; init; }

    public IReadOnlyList<Guid> DisableKeys { get; init; } = new List<Guid>();

    public IReadOnlyList<Guid> ImmediateDisableKeys { get; init; } = new List<Guid>();
}
