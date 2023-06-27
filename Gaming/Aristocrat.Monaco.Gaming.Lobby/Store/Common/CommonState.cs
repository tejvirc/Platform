namespace Aristocrat.Monaco.Gaming.Lobby.Store.Application;

using System;
using Fluxor;

[FeatureState]
public record CommonState
{
    public IntPtr GameMainHandle { get; set; }

    public IntPtr GameTopHandle { get; set; }

    public IntPtr GameTopperHandle { get; set; }

    public IntPtr GameButtonDeckHandle { get; set; }

    public bool IsSystemDisabled { get; set; }

    public bool IsSystemDisableImmediately { get; set; }
}
