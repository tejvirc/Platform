namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using System;

public class GameBottomWindowLoadedAction
{
    public GameBottomWindowLoadedAction(IntPtr windowHandle)
    {
        WindowHandle = windowHandle;
    }

    public IntPtr WindowHandle { get; }
}
