namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using System;

public class GameTopWindowLoadedAction
{
    public GameTopWindowLoadedAction(IntPtr handle)
    {
        Handle = handle;
    }

    public IntPtr Handle { get; }
}
