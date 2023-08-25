namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using System;

public class GameMainWindowLoadedAction
{
    public GameMainWindowLoadedAction(IntPtr windowHandle)
    {
        WindowHandle = windowHandle;
    }

    public IntPtr WindowHandle { get; }
}
