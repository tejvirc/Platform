namespace Aristocrat.Monaco.Hardware.Usb
{
    using System;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        #region Internal Function Imports
            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "LibK_GetVersion", SetLastError = true)]
            public static extern void LibK_GetVersion([Out] out KLibVersion version);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "LibK_GetContext", SetLastError = true)]
            public static extern IntPtr LibK_GetContext([In] IntPtr handle, KlibHandleType handleType);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "LibK_SetContext", SetLastError = true)]
            public static extern bool LibK_SetContext([In] IntPtr handle, KlibHandleType handleType, IntPtr contextValue);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "LibK_SetCleanupCallback",
                SetLastError = true)]
            public static extern bool LibK_SetCleanupCallback([In] IntPtr handle, KlibHandleType handleType, KLibHandleCleanupCb cleanupCb);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "LibK_LoadDriverAPI", SetLastError = true)]
            public static extern bool LibK_LoadDriverAPI([Out] out KUsbDriverApi driverApi, int driverId);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "LibK_CopyDriverAPI", SetLastError = true)]
            public static extern bool LibK_CopyDriverAPI([Out] out KUsbDriverApi driverApi, [In] KusbHandle usbHandle);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "LibK_GetProcAddress", SetLastError = true)]
            public static extern bool LibK_GetProcAddress(IntPtr procAddress, int driverId, int functionId);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_Init", SetLastError = true)]
            public static extern bool UsbK_Init([Out] out KusbHandle interfaceHandle, [In] KLstDevInfoHandle devInfo);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_Free", SetLastError = true)]
            public static extern bool UsbK_Free([In] IntPtr interfaceHandle);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_ClaimInterface", SetLastError = true)]
            public static extern bool UsbK_ClaimInterface([In] KusbHandle interfaceHandle, byte numberOrIndex, bool isIndex);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_ReleaseInterface", SetLastError = true)
            ]
            public static extern bool UsbK_ReleaseInterface([In] KusbHandle interfaceHandle, byte numberOrIndex, bool isIndex);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_SetAltInterface", SetLastError = true)]
            public static extern bool UsbK_SetAltInterface([In] KusbHandle interfaceHandle, byte numberOrIndex, bool isIndex, byte altSettingNumber);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_GetAltInterface", SetLastError = true)]
            public static extern bool UsbK_GetAltInterface([In] KusbHandle interfaceHandle, byte numberOrIndex, bool isIndex, out byte altSettingNumber);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_GetDescriptor", SetLastError = true)]
            public static extern bool UsbK_GetDescriptor([In] KusbHandle interfaceHandle,
                                                         byte descriptorType,
                                                         byte index,
                                                         ushort languageId,
                                                         IntPtr buffer,
                                                         uint bufferLength,
                                                         out uint lengthTransferred);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_ControlTransfer", SetLastError = true)]
            public static extern bool UsbK_ControlTransfer([In] KusbHandle interfaceHandle,
                                                           WinUsbSetupPacket setupPacket,
                                                           IntPtr buffer,
                                                           uint bufferLength,
                                                           out uint lengthTransferred,
                                                           IntPtr overlapped);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_SetPowerPolicy", SetLastError = true)]
            public static extern bool UsbK_SetPowerPolicy([In] KusbHandle interfaceHandle, uint policyType, uint valueLength, IntPtr value);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_GetPowerPolicy", SetLastError = true)]
            public static extern bool UsbK_GetPowerPolicy([In] KusbHandle interfaceHandle, uint policyType, ref uint valueLength, IntPtr value);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_SetConfiguration", SetLastError = true)
            ]
            public static extern bool UsbK_SetConfiguration([In] KusbHandle interfaceHandle, byte configurationNumber);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_GetConfiguration", SetLastError = true)
            ]
            public static extern bool UsbK_GetConfiguration([In] KusbHandle interfaceHandle, out byte configurationNumber);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_ResetDevice", SetLastError = true)]
            public static extern bool UsbK_ResetDevice([In] KusbHandle interfaceHandle);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_Initialize", SetLastError = true)]
            public static extern bool UsbK_Initialize(IntPtr deviceHandle, [Out] out KusbHandle interfaceHandle);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_SelectInterface", SetLastError = true)]
            public static extern bool UsbK_SelectInterface([In] KusbHandle interfaceHandle, byte numberOrIndex, bool isIndex);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_GetAssociatedInterface",
                SetLastError = true)]
            public static extern bool UsbK_GetAssociatedInterface([In] KusbHandle interfaceHandle,
                                                                  byte associatedInterfaceIndex,
                                                                  [Out] out KusbHandle associatedInterfaceHandle);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_Clone", SetLastError = true)]
            public static extern bool UsbK_Clone([In] KusbHandle interfaceHandle, [Out] out KusbHandle dstInterfaceHandle);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_QueryInterfaceSettings",
                SetLastError = true)]
            public static extern bool UsbK_QueryInterfaceSettings([In] KusbHandle interfaceHandle,
                                                                  byte altSettingNumber,
                                                                  [Out] out UsbInterfaceDescriptor usbAltInterfaceDescriptor);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_QueryDeviceInformation",
                SetLastError = true)]
            public static extern bool UsbK_QueryDeviceInformation([In] KusbHandle interfaceHandle, uint informationType, ref uint bufferLength, IntPtr buffer);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_SetCurrentAlternateSetting",
                SetLastError = true)]
            public static extern bool UsbK_SetCurrentAlternateSetting([In] KusbHandle interfaceHandle, byte altSettingNumber);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_GetCurrentAlternateSetting",
                SetLastError = true)]
            public static extern bool UsbK_GetCurrentAlternateSetting([In] KusbHandle interfaceHandle, out byte altSettingNumber);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_QueryPipe", SetLastError = true)]
            public static extern bool UsbK_QueryPipe([In] KusbHandle interfaceHandle,
                                                     byte altSettingNumber,
                                                     byte pipeIndex,
                                                     [Out] out WinUsbPipeInformation pipeInformation);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_SetPipePolicy", SetLastError = true)]
            public static extern bool UsbK_SetPipePolicy([In] KusbHandle interfaceHandle, byte pipeId, uint policyType, uint valueLength, IntPtr value);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_GetPipePolicy", SetLastError = true)]
            public static extern bool UsbK_GetPipePolicy([In] KusbHandle interfaceHandle, byte pipeId, uint policyType, ref uint valueLength, IntPtr value);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_ReadPipe", SetLastError = true)]
            public static extern bool UsbK_ReadPipe([In] KusbHandle interfaceHandle,
                                                    byte pipeId,
                                                    IntPtr buffer,
                                                    uint bufferLength,
                                                    out uint lengthTransferred,
                                                    IntPtr overlapped);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_WritePipe", SetLastError = true)]
            public static extern bool UsbK_WritePipe([In] KusbHandle interfaceHandle,
                                                     byte pipeId,
                                                     IntPtr buffer,
                                                     uint bufferLength,
                                                     out uint lengthTransferred,
                                                     IntPtr overlapped);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_ResetPipe", SetLastError = true)]
            public static extern bool UsbK_ResetPipe([In] KusbHandle interfaceHandle, byte pipeId);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_AbortPipe", SetLastError = true)]
            public static extern bool UsbK_AbortPipe([In] KusbHandle interfaceHandle, byte pipeId);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_FlushPipe", SetLastError = true)]
            public static extern bool UsbK_FlushPipe([In] KusbHandle interfaceHandle, byte pipeId);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_IsoReadPipe", SetLastError = true)]
            public static extern bool UsbK_IsoReadPipe([In] KusbHandle interfaceHandle,
                                                       byte pipeId,
                                                       IntPtr buffer,
                                                       uint bufferLength,
                                                       IntPtr overlapped,
                                                       [In] KisoContext isoContext);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_IsoWritePipe", SetLastError = true)]
            public static extern bool UsbK_IsoWritePipe([In] KusbHandle interfaceHandle,
                                                        byte pipeId,
                                                        IntPtr buffer,
                                                        uint bufferLength,
                                                        IntPtr overlapped,
                                                        [In] KisoContext isoContext);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_GetCurrentFrameNumber",
                SetLastError = true)]
            public static extern bool UsbK_GetCurrentFrameNumber([In] KusbHandle interfaceHandle, out uint frameNumber);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_GetOverlappedResult",
                SetLastError = true)]
            public static extern bool UsbK_GetOverlappedResult([In] KusbHandle interfaceHandle, IntPtr overlapped, out uint lpNumberOfBytesTransferred, bool bWait);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "UsbK_GetProperty", SetLastError = true)]
            public static extern bool UsbK_GetProperty([In] KusbHandle interfaceHandle, KUsbProperty propertyType, ref uint propertySize, IntPtr value);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "LstK_Init", SetLastError = true)]
            public static extern bool LstK_Init([Out] out KlstHandle deviceList, KlstFlag flags);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "LstK_InitEx", SetLastError = true)]
            public static extern bool LstK_InitEx([Out] out KlstHandle deviceList, KlstFlag flags, [In] ref KLstPatternMatch patternMatch);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "LstK_Free", SetLastError = true)]
            public static extern bool LstK_Free([In] IntPtr deviceList);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "LstK_Enumerate", SetLastError = true)]
            public static extern bool LstK_Enumerate([In] KlstHandle deviceList, KLstEnumDevInfoCb enumDevListCb, IntPtr context);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "LstK_Current", SetLastError = true)]
            public static extern bool LstK_Current([In] KlstHandle deviceList, [Out] out KLstDevInfoHandle deviceInfo);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "LstK_MoveNext", SetLastError = true)]
            public static extern bool LstK_MoveNext([In] KlstHandle deviceList, [Out] out KLstDevInfoHandle deviceInfo);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "LstK_MoveReset", SetLastError = true)]
            public static extern void LstK_MoveReset([In] KlstHandle deviceList);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "LstK_FindByVidPid", SetLastError = true)]
            public static extern bool LstK_FindByVidPid([In] KlstHandle deviceList, int vid, int pid, [Out] out KLstDevInfoHandle deviceInfo);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "LstK_Count", SetLastError = true)]
            public static extern bool LstK_Count([In] KlstHandle deviceList, ref uint count);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "HotK_Init", SetLastError = true)]
            public static extern bool HotK_Init([Out] out KhotHandle handle, [In, Out] ref KHotParams initParams);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "HotK_Free", SetLastError = true)]
            public static extern bool HotK_Free([In] IntPtr handle);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "HotK_FreeAll", SetLastError = true)]
            public static extern void HotK_FreeAll();

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "OvlK_Acquire", SetLastError = true)]
            public static extern bool OvlK_Acquire([Out] out KovlHandle overlappedK, [In] KovlPoolHandle poolHandle);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "OvlK_Release", SetLastError = true)]
            public static extern bool OvlK_Release([In] KovlHandle overlappedK);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "OvlK_Init", SetLastError = true)]
            public static extern bool OvlK_Init([Out] out KovlPoolHandle poolHandle, [In] KusbHandle usbHandle, int maxOverlappedCount, KOvlPoolFlag flags);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "OvlK_Free", SetLastError = true)]
            public static extern bool OvlK_Free([In] IntPtr poolHandle);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "OvlK_GetEventHandle", SetLastError = true)]
            public static extern IntPtr OvlK_GetEventHandle([In] KovlHandle overlappedK);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "OvlK_Wait", SetLastError = true)]
            public static extern bool OvlK_Wait([In] KovlHandle overlappedK, int timeoutMs, KOvlWaitFlag waitFlags, out uint transferredLength);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "OvlK_WaitOrCancel", SetLastError = true)]
            public static extern bool OvlK_WaitOrCancel([In] KovlHandle overlappedK, int timeoutMs, out uint transferredLength);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "OvlK_WaitAndRelease", SetLastError = true)]
            public static extern bool OvlK_WaitAndRelease([In] KovlHandle overlappedK, int timeoutMs, out uint transferredLength);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "OvlK_IsComplete", SetLastError = true)]
            public static extern bool OvlK_IsComplete([In] KovlHandle overlappedK);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "OvlK_ReUse", SetLastError = true)]
            public static extern bool OvlK_ReUse([In] KovlHandle overlappedK);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "StmK_Init", SetLastError = true)]
            public static extern bool StmK_Init([Out] out KstmHandle streamHandle,
                                                [In] KusbHandle usbHandle,
                                                byte pipeId,
                                                int maxTransferSize,
                                                int maxPendingTransfers,
                                                int maxPendingIo,
                                                [In] ref KStmCallback callbacks,
                                                KStmFlag flags);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "StmK_Free", SetLastError = true)]
            public static extern bool StmK_Free([In] IntPtr streamHandle);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "StmK_Start", SetLastError = true)]
            public static extern bool StmK_Start([In] KstmHandle streamHandle);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "StmK_Stop", SetLastError = true)]
            public static extern bool StmK_Stop([In] KstmHandle streamHandle, int timeoutCancelMs);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "StmK_Read", SetLastError = true)]
            public static extern bool StmK_Read([In] KstmHandle streamHandle, IntPtr buffer, int offset, int length, out uint transferredLength);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "StmK_Write", SetLastError = true)]
            public static extern bool StmK_Write([In] KstmHandle streamHandle, IntPtr buffer, int offset, int length, out uint transferredLength);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "IsoK_Init", SetLastError = true)]
            public static extern bool IsoK_Init([Out] out KisoContext isoContext, int numberOfPackets, int startFrame);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "IsoK_Free", SetLastError = true)]
            public static extern bool IsoK_Free([In] IntPtr isoContext);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "IsoK_SetPackets", SetLastError = true)]
            public static extern bool IsoK_SetPackets([In] KisoContext isoContext, int packetSize);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "IsoK_SetPacket", SetLastError = true)]
            public static extern bool IsoK_SetPacket([In] KisoContext isoContext, int packetIndex, [In] ref KIsoPacket isoPacket);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "IsoK_GetPacket", SetLastError = true)]
            public static extern bool IsoK_GetPacket([In] KisoContext isoContext, int packetIndex, [Out] out KIsoPacket isoPacket);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "IsoK_EnumPackets", SetLastError = true)]
            public static extern bool IsoK_EnumPackets([In] KisoContext isoContext, KIsoEnumPacketsCb enumPackets, int startPacketIndex, IntPtr userState);

            [DllImport(Constants.LibusbkDll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "IsoK_ReUse", SetLastError = true)]
            public static extern bool IsoK_ReUse([In] KisoContext isoContext);
        #endregion
    }
}
