namespace Aristocrat.Monaco.Gaming.Lobby.Store.Runtime;

using System;
using Fluxor;

[FeatureState]
public record RuntimeState
{
    public IntPtr GameMainHandle { get; set; }

    public IntPtr GameTopHandle { get; set; }

    public IntPtr GameTopperHandle { get; set; }

    public IntPtr GameButtonDeckHandle { get; set; }
}
