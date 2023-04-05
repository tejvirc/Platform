namespace Lobby;

using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

public class PlatformHost : HwndHost
{
    private readonly IntPtr _handle;

    public PlatformHost(IntPtr handle)
    {
        _handle = handle;
    }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        return new HandleRef(this, _handle);
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
    }
}
