namespace Aristocrat.Sas.Client.SerialComm.Win32Comm
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    /// <summary>
    ///     Groups the native Win32 APIs used for the serial communications
    /// </summary>
    internal class NativeMethods
    {
        private NativeMethods() { }

        /// <summary> Opening, Testing and Closing the Port Handle. </summary>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        /// <summary> Constants for errors </summary>
        internal const uint ErrorFileNotFound = 2;
        internal const uint ErrorInvalidName = 123;
        internal const uint ErrorAccessDenied = 5;
        internal const uint ErrorIoPending = 997;

        /// <summary> Constants for return value </summary>
        internal const int InvalidHandleValue = -1;

        /// <summary> Constants for dwFlagsAndAttributes </summary>
        internal const uint FileFlagOverlapped = 0x40000000;
        internal const uint FileFlagNoBuffering = 0x20000000;

        /// <summary> Constants for dwCreationDisposition </summary>
        internal const uint OpenExisting = 3;

        /// <summary> Constants for dwDesiredAccess </summary>
        internal const uint GenericRead = 0x80000000;
        internal const uint GenericWrite = 0x40000000;

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetHandleInformation(SafeFileHandle hObject, out uint lpdwFlags);

        /// <summary> Manipulating the communications settings / DTR Control Flow values</summary>
        internal const int DtrControlDisable = 0x00;
        internal const int DtrControlEnable = 0x01;
        internal const int DtrControlHandshake = 0x02;

        /// <summary> Manipulating the communications settings / RTS Control Flow values </summary>
        internal const int RtsControlDisable = 0x00;
        internal const int RtsControlEnable = 0x01;
        internal const int RtsControlHandshake = 0x02;
        internal const int RtsControlToggle = 0x03;

        /// <summary> Manipulating the communications settings / functions </summary>
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCommState(SafeFileHandle hFile, ref Dcb lpDcb);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetCommState(SafeFileHandle hFile, [In] ref Dcb lpDcb);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetCommTimeouts(SafeFileHandle hFile, [In] ref CommTimeouts lpCommTimeouts);

        /// <summary> Reading, writing and purging</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern unsafe bool WriteFile(
            SafeFileHandle hFile,
            byte[] lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            NativeOverlapped* lpOverlapped);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetCommMask(SafeFileHandle hFile, uint dwEvtMask);

        /// <summary> Constants for dwEvtMask </summary>
        internal const uint EvRxChar = 0x0001;
        internal const uint EvRxFlag = 0x0002;
        internal const uint EvTxEmpty = 0x0004;
        internal const uint EvCts = 0x0008;
        internal const uint EvDsr = 0x0010;
        internal const uint EvRlsd = 0x0020;
        internal const uint EvBreak = 0x0040;
        internal const uint EvErr = 0x0080;
        internal const uint EvRing = 0x0100;
        internal const uint EvPerr = 0x0200;
        internal const uint EvRx80Full = 0x0400;
        internal const uint EvEvent1 = 0x0800;
        internal const uint EvEvent2 = 0x1000;

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern unsafe bool WaitCommEvent(
            SafeFileHandle hFile,
            IntPtr lpEvtMask,
            NativeOverlapped* lpOverlapped);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CancelIo(SafeFileHandle hFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern unsafe bool ReadFile(
            SafeFileHandle hFile,
            [Out] byte[] lpBuffer,
            uint nNumberOfBytesToRead,
            out uint nNumberOfBytesRead,
            NativeOverlapped* lpOverlapped);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool TransmitCommChar(SafeFileHandle hFile, byte cChar);

        /// <summary> Port status Functions </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern unsafe bool GetOverlappedResult(
            SafeFileHandle hFile,
            NativeOverlapped* lpOverlapped,
            out uint nNumberOfBytesTransferred,
            [MarshalAs(UnmanagedType.Bool)]bool bWait);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ClearCommError(SafeFileHandle hFile, out uint lpErrors, out ComStat cs);

        /// <summary> Constants for lpErrors </summary>
        internal const uint CeRxOver = 0x0001;
        internal const uint CeOverrun = 0x0002;
        internal const uint CeRxParity = 0x0004;
        internal const uint CeFrame = 0x0008;
        internal const uint CeBreak = 0x0010;
        internal const uint CeTxFull = 0x0100;
        internal const uint CePto = 0x0200;
        internal const uint CeIoe = 0x0400;
        internal const uint CeDns = 0x0800;
        internal const uint CeOop = 0x1000;
        internal const uint CeMode = 0x8000;

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetupComm(SafeFileHandle hFile, uint dwInQueue, uint dwOutQueue);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool PurgeComm(SafeFileHandle hFile, uint dwFlags);

        /// <summary> Constants for dwFlags </summary>
        internal const uint PurgeTxAbort = 0x0001;
        internal const uint PurgeRxAbort = 0x0002;
        internal const uint PurgeTxClear = 0x0004;
        internal const uint PurgeRxClear = 0x0008;

        /// <summary> API for wait object event </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern uint WaitForSingleObject(SafeWaitHandle hHandle, uint dwMilliseconds);

        /// <summary> Constants for WaitForSingleObject's return value </summary>
        internal const uint StatusWait0 = 0x0;
        internal const uint WaitObject0 = StatusWait0 + 0;
        internal const uint WaitTimeout = 258;
    }
}
