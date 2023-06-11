namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using System;
using System.Collections.Immutable;
using Kernel;

public record SystemDisabledAction
{
    public bool IsDisabled { get; init; }

    public bool IsDisableImmediately { get; init; }

    public SystemDisablePriority Priority { get; init; }

    public IImmutableList<Guid>? DisableKeys { get; init; }

    public IImmutableList<Guid>? ImmediateDisableKeys { get; init; }
}
