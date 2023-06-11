namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using System.Collections.Immutable;
using System;

public class SystemEnabledAction
{
    public bool IsDisabled { get; init; }

    public bool IsDisableImmediately { get; init; }

    public IImmutableList<Guid>? DisableKeys { get; init; }

    public IImmutableList<Guid>? ImmediateDisableKeys { get; init; }
}
