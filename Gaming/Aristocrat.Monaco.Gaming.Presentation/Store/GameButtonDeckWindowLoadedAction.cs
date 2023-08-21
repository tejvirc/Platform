namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using System;

public class GameButtonDeckWindowLoadedAction
{
    public GameButtonDeckWindowLoadedAction(IntPtr windowHandle)
    {
        WindowHandle = windowHandle;
    }

    public IntPtr WindowHandle { get; }
}
