namespace Aristocrat.Monaco.Gaming.Lobby.Store.Application;

using System;

public class CommonReducers
{
    public bool IsSystemDisabled { get; set; }

    public bool IsSystemDisableImmediately { get; set; }

    public IntPtr GameMainHandle { get; set; }

    public IntPtr GameTopHandle { get; set; }

    public IntPtr GameTopperHandle { get; set; }

    public IntPtr GameButtonDeckHandle { get; set; }
}
