﻿namespace Aristocrat.Monaco.Hardware.Audio
{
    using System;
    using System.Runtime.InteropServices;

    [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISimpleAudioVolume
    {
        [PreserveSig]
        int SetMasterVolume(float fLevel, ref Guid eventContext);

        [PreserveSig]
        int GetMasterVolume(out float pfLevel);

        [PreserveSig]
        int SetMute(bool bMute, ref Guid eventContext);

        [PreserveSig]
        int GetMute(out bool pbMute);
    }
}