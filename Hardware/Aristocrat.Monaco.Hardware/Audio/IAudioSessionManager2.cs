namespace Aristocrat.Monaco.Hardware.Audio
{
    using System.Runtime.InteropServices;

    [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionManager2
    {
        int NotImplemented();

        int Notimplemented2();

        [PreserveSig]
        int GetSessionEnumerator(out IAudioSessionEnumerator sessionEnum);

        // the rest is not implemented
    }
}