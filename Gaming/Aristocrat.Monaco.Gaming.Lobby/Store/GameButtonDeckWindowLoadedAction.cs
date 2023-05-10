namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using System;

public class GameButtonDeckWindowLoadedAction
{
    public GameButtonDeckWindowLoadedAction(IntPtr handle)
    {
        Handle = handle;
    }

    public IntPtr Handle { get; }
}
