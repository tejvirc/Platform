namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using System;

public class GameTopperWindowLoadedAction
{
    public GameTopperWindowLoadedAction(IntPtr windowHandle)
    {
        WindowHandle = windowHandle;
    }

    public IntPtr WindowHandle { get; }
}
