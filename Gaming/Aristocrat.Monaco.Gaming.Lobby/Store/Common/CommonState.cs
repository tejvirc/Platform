namespace Aristocrat.Monaco.Gaming.Lobby.Store.Common;

using System;
using Fluxor;

[FeatureState]
public record CommonState
{
    public bool IsSystemDisabled { get; set; }

    public bool IsSystemDisableImmediately { get; set; }
}
