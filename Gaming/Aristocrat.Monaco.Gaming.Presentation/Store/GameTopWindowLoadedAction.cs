namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using System;

public class GameTopWindowLoadedAction
{
    public GameTopWindowLoadedAction(IntPtr windowHandle)
    {
        WindowHandle = windowHandle;
    }

    public IntPtr WindowHandle { get; }
}
