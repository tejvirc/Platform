namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using System;

public class GameMainWindowLoadedAction
{
    public GameMainWindowLoadedAction(IntPtr handle)
    {
        Handle = handle;
    }

    public IntPtr Handle { get; }
}
