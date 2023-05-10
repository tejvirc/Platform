namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using System;

public class GameTopperWindowLoadedAction
{
    public GameTopperWindowLoadedAction(IntPtr handle)
    {
        Handle = handle;
    }

    public IntPtr Handle { get; }
}
