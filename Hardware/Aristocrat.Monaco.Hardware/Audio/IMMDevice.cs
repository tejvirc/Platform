namespace Aristocrat.Monaco.Hardware.Audio
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    ///     An audio service provider.
    /// </summary>
    public class MultimediaDevice
    {
    }

    [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
        [PreserveSig]
        int Activate(
            ref Guid iid,
            int dwClsCtx,
            IntPtr pActivationParams,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);

        [PreserveSig]
        int OpenPropertyStore(
            uint stgmAccess,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppProperties);

        [PreserveSig]
        int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);

        [PreserveSig]
        int GetState(out DeviceState pdwState);
    }
}