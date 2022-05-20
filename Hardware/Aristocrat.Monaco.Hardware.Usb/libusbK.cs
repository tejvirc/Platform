#region Copyright (c) Travis Robinson
// Copyright (c) 2011 Travis Robinson <libusbdotnet@gmail.com>
// All rights reserved.
//
// C# libusbK Bindings
// Auto-generated on: 01.28.2012
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS
// IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
// TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
// PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL TRAVIS LEE ROBINSON
// BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
#endregion

// Disable compile warnings
#pragma warning disable 1591
#pragma warning disable 649
namespace Aristocrat.Monaco.Hardware.Usb
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Microsoft.Win32.SafeHandles;

    public static class Constants
    {
        /// <summary>
        ///   Allocated length for all strings in a \ref KLST_DEVINFO structure.
        /// </summary>
        public const int KlstStringMaxLen = 256;

        /// <summary>
        ///   libusbK library
        /// </summary>
        public const string LibusbkDll = "libusbK.dll";

        /// <summary>
        ///   Config power mask for the \c bmAttributes field of a \ref USB_CONFIGURATION_DESCRIPTOR
        /// </summary>
        public const byte UsbConfigPoweredMask = 0xC0;

        /// <summary>
        ///   Endpoint direction mask for the \c bEndpointAddress field of a \ref USB_ENDPOINT_DESCRIPTOR
        /// </summary>
        public const byte UsbEndpointDirectionMask = 0x80;

        /// <summary>
        ///   Endpoint address mask for the \c bEndpointAddress field of a \ref USB_ENDPOINT_DESCRIPTOR
        /// </summary>
        public const byte UsbEndpointAddressMask = 0x0F;
    }

    public enum PipePolicyType
    {
        ShortPacketTerminate = 0x01,
        AutoClearStall = 0x02,
        PipeTransferTimeout = 0x03,
        IgnoreShortPackets = 0x04,
        AllowPartialReads = 0x05,
        AutoFlush = 0x06,
        RawIo = 0x07,
        MaximumTransferSize = 0x08,
        ResetPipeOnResume = 0x09,

        IsoStartLatency = 0x20,
        IsoAlwaysStartAsap = 0x21,
        IsoNumFixedPackets = 0x22,

        SimulParallelRequests = 0x30,
    }

    public enum PowerPolicyType
    {
        AutoSuspend = 0x81,
        SuspendDelay = 0x83,
    }

    public enum DeviceInformationType
    {
        DeviceSpeed = 0x01,
    }

    public enum EndpointType
    {
        /// <summary>
        ///   Indicates a control endpoint
        /// </summary>
        Control = 0x00,

        /// <summary>
        ///   Indicates an iso endpoint
        /// </summary>
        Isochronous = 0x01,

        /// <summary>
        ///   Indicates a bulk endpoint
        /// </summary>
        Bulk = 0x02,

        /// <summary>
        ///   Indicates an interrupt endpoint
        /// </summary>
        Interrupt = 0x03,

        /// <summary>
        ///   Endpoint type mask for the \c bmAttributes field of a \ref USB_ENDPOINT_DESCRIPTOR
        /// </summary>
        Mask = 0x03
    }

    public static class ErrorCodes
    {
        /// <summary>
        ///   The operation completed successfully.
        /// </summary>
        public const int Success = 0;

        /// <summary>
        ///   Access is denied.
        /// </summary>
        public const int AccessDenied = 5;

        /// <summary>
        ///   The handle is invalid.
        /// </summary>
        public const int InvalidHandle = 6;

        /// <summary>
        ///   Not enough storage is available to process this command.
        /// </summary>
        public const int NotEnoughMemory = 8;

        /// <summary>
        ///   The request is not supported.
        /// </summary>
        public const int NotSupported = 50;

        /// <summary>
        ///   The parameter is incorrect.
        /// </summary>
        public const int InvalidParameter = 87;

        /// <summary>
        ///   The semaphore timeout period has expired.
        /// </summary>
        public const int SemTimeout = 121;

        /// <summary>
        ///   The requested resource is in use.
        /// </summary>
        public const int Busy = 170;

        /// <summary>
        ///   Too many dynamic-link modules are attached to this program or dynamic-link module.
        /// </summary>
        public const int TooManyModules = 214;

        /// <summary>
        ///   More data is available.
        /// </summary>
        public const int MoreData = 234;

        /// <summary>
        ///   No more data is available.
        /// </summary>
        public const int NoMoreItems = 259;

        /// <summary>
        ///   An attempt was made to operate on a thread within a specific process, but the thread specified is not in the process specified.
        /// </summary>
        public const int ThreadNotInProcess = 566;

        /// <summary>
        ///   A thread termination occurred while the thread was suspended. The thread was resumed, and termination proceeded.
        /// </summary>
        public const int ThreadWasSuspended = 699;

        /// <summary>
        ///   The I/O operation has been aborted because of either a thread exit or an application request.
        /// </summary>
        public const int OperationAborted = 995;

        /// <summary>
        ///   Overlapped I/O event is not in a signaled state.
        /// </summary>
        public const int IoIncomplete = 996;

        /// <summary>
        ///   Overlapped I/O operation is in progress.
        /// </summary>
        public const int IoPending = 997;

        /// <summary>
        ///   Element not found.
        /// </summary>
        public const int NotFound = 1168;

        /// <summary>
        ///   The operation was canceled by the user.
        /// </summary>
        public const int Cancelled = 1223;

        /// <summary>
        ///   The library, drive, or media pool is empty.
        /// </summary>
        public const int Empty = 4306;

        /// <summary>
        ///   The cluster resource is not available.
        /// </summary>
        public const int ResourceNotAvailable = 5006;

        /// <summary>
        ///   The cluster resource could not be found.
        /// </summary>
        public const int ResourceNotFound = 5007;
    }

    #region Base KLIB Handle
    public abstract class KlibHandle : SafeHandleZeroOrMinusOneIsInvalid, IKlibHandle
    {
        protected KlibHandle(bool ownsHandle) : base(ownsHandle)
        {
        }

        protected KlibHandle() : this(true)
        {
        }

        protected KlibHandle(IntPtr handlePtrToWrap) : base(false)
        {
            SetHandle(handlePtrToWrap);
        }


        #region IKLIB_HANDLE Members
        public abstract KlibHandleType HandleType { get; }

        public IntPtr GetContext()
        {
            return NativeMethods.LibK_GetContext(handle, HandleType);
        }

        public bool SetContext(IntPtr contextValue)
        {
            return NativeMethods.LibK_SetContext(handle, HandleType, contextValue);
        }

        public bool SetCleanupCallback(KLibHandleCleanupCb cleanupCallback)
        {
            return NativeMethods.LibK_SetCleanupCallback(handle, HandleType, cleanupCallback);
        }
        #endregion


        protected abstract override bool ReleaseHandle();
    }

    public interface IKlibHandle
    {
        KlibHandleType HandleType { get; }
        IntPtr DangerousGetHandle();
        IntPtr GetContext();
        bool SetContext(IntPtr userContext);
        bool SetCleanupCallback(KLibHandleCleanupCb cleanupCallback);
    }
    #endregion

    #region Class Handles
    public class KhotHandle : KlibHandle
    {
        public static KhotHandle Empty => new KhotHandle();

        public override KlibHandleType HandleType => KlibHandleType.HotK;

        protected override bool ReleaseHandle()
        {
            return NativeMethods.HotK_Free(handle);
        }
    }

    public class KusbHandle : KlibHandle
    {
        public static KusbHandle Empty => new KusbHandle();

        public override KlibHandleType HandleType => KlibHandleType.UsbK;

        protected override bool ReleaseHandle()
        {
            {
                return NativeMethods.UsbK_Free(handle);
            }
        }


        #region USB Shared Device Context
        public IntPtr GetSharedContext()
        {
            return NativeMethods.LibK_GetContext(handle, KlibHandleType.UsbSharedK);
        }

        public bool SetSharedContext(IntPtr sharedUserContext)
        {
            return NativeMethods.LibK_SetContext(handle, KlibHandleType.UsbSharedK, sharedUserContext);
        }

        public bool SetSharedCleanupCallback(KLibHandleCleanupCb cleanupCallback)
        {
            return NativeMethods.LibK_SetCleanupCallback(handle, KlibHandleType.UsbSharedK, cleanupCallback);
        }
        #endregion
    }

    public class KlstHandle : KlibHandle
    {
        public static KlstHandle Empty => new KlstHandle();

        public override KlibHandleType HandleType => KlibHandleType.LstK;

        protected override bool ReleaseHandle()
        {
            return NativeMethods.LstK_Free(handle);
        }
    }

    public class KovlPoolHandle : KlibHandle
    {
        public static KovlPoolHandle Empty => new KovlPoolHandle();

        public override KlibHandleType HandleType => KlibHandleType.OvlPoolK;

        protected override bool ReleaseHandle()
        {
            return NativeMethods.OvlK_Free(handle);
        }
    }

    public class KstmHandle : KlibHandle
    {
        public static KstmHandle Empty => new KstmHandle();

        public override KlibHandleType HandleType => KlibHandleType.StmK;

        protected override bool ReleaseHandle()
        {
            return NativeMethods.StmK_Free(handle);
        }
    }

    public class KisoContext : SafeHandleZeroOrMinusOneIsInvalid
    {
        protected KisoContext(bool ownsHandle) : base(ownsHandle)
        {
        }

        protected KisoContext() : this(true)
        {
        }

        protected KisoContext(IntPtr handlePtrToWrap) : base(false)
        {
            SetHandle(handlePtrToWrap);
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.IsoK_Free(handle);
            handle = IntPtr.Zero;
            return true;
        }
    }
    #endregion

    #region Struct Handles
    public struct KovlHandle : IKlibHandle
    {
        private readonly IntPtr handle;

        public static KovlHandle Empty => new KovlHandle();

        #region IKLIB_HANDLE Members
        public KlibHandleType HandleType => KlibHandleType.OvlK;

        public IntPtr GetContext()
        {
            return NativeMethods.LibK_GetContext(handle, HandleType);
        }

        public bool SetContext(IntPtr contextValue)
        {
            return NativeMethods.LibK_SetContext(handle, HandleType, contextValue);
        }

        public bool SetCleanupCallback(KLibHandleCleanupCb cleanupCallback)
        {
            return NativeMethods.LibK_SetCleanupCallback(handle, HandleType, cleanupCallback);
        }

        public IntPtr DangerousGetHandle()
        {
            return handle;
        }
        #endregion
    }

    public struct KLstDevInfoHandle : IKlibHandle
    {
        private static readonly int OfsClassGuid = Marshal.OffsetOf(typeof (KLstDevinfo), "ClassGUID").ToInt32();
        private static readonly int OfsCommon = Marshal.OffsetOf(typeof (KLstDevinfo), "Common").ToInt32();
        private static readonly int OfsConnected = Marshal.OffsetOf(typeof (KLstDevinfo), "Connected").ToInt32();
        private static readonly int OfsDeviceDesc = Marshal.OffsetOf(typeof (KLstDevinfo), "DeviceDesc").ToInt32();
        private static readonly int OfsDeviceInterfaceGuid = Marshal.OffsetOf(typeof (KLstDevinfo), "DeviceInterfaceGUID").ToInt32();
        private static readonly int OfsDevicePath = Marshal.OffsetOf(typeof (KLstDevinfo), "DevicePath").ToInt32();
        private static readonly int OfsDriverId = Marshal.OffsetOf(typeof (KLstDevinfo), "DriverID").ToInt32();
        private static readonly int OfsInstanceId = Marshal.OffsetOf(typeof (KLstDevinfo), "InstanceID").ToInt32();
        private static readonly int OfsLUsb0FilterIndex = Marshal.OffsetOf(typeof (KLstDevinfo), "LUsb0FilterIndex").ToInt32();
        private static readonly int OfsMfg = Marshal.OffsetOf(typeof (KLstDevinfo), "Mfg").ToInt32();
        private static readonly int OfsService = Marshal.OffsetOf(typeof (KLstDevinfo), "Service").ToInt32();
        private static readonly int OfsSymbolicLink = Marshal.OffsetOf(typeof (KLstDevinfo), "SymbolicLink").ToInt32();
        private static readonly int OfsSyncFlags = Marshal.OffsetOf(typeof (KLstDevinfo), "SyncFlags").ToInt32();

        private readonly IntPtr handle;

        public KLstDevInfoHandle(IntPtr handleToWrap)
        {
            handle = handleToWrap;
        }

        public static KLstDevInfoHandle Empty => new KLstDevInfoHandle();

        public KlstSyncFlag SyncFlags => (KlstSyncFlag) Marshal.ReadInt32(handle, OfsSyncFlags);

        public bool Connected => Marshal.ReadInt32(handle, OfsConnected) != 0;

        public int LUsb0FilterIndex => Marshal.ReadInt32(handle, OfsLUsb0FilterIndex);

        public string DevicePath => Marshal.PtrToStringAnsi(new IntPtr(handle.ToInt64() + OfsDevicePath));

        public string SymbolicLink => Marshal.PtrToStringAnsi(new IntPtr(handle.ToInt64() + OfsSymbolicLink));

        public string Service => Marshal.PtrToStringAnsi(new IntPtr(handle.ToInt64() + OfsService));

        public string DeviceDesc => Marshal.PtrToStringAnsi(new IntPtr(handle.ToInt64() + OfsDeviceDesc));

        public string Mfg => Marshal.PtrToStringAnsi(new IntPtr(handle.ToInt64() + OfsMfg));

        public string ClassGuid => Marshal.PtrToStringAnsi(new IntPtr(handle.ToInt64() + OfsClassGuid));

        public string InstanceId => Marshal.PtrToStringAnsi(new IntPtr(handle.ToInt64() + OfsInstanceId));

        public string DeviceInterfaceGuid => Marshal.PtrToStringAnsi(new IntPtr(handle.ToInt64() + OfsDeviceInterfaceGuid));

        public int DriverId => Marshal.ReadInt32(handle, OfsDriverId);

        public KLstDevCommonInfo Common => (KLstDevCommonInfo) Marshal.PtrToStructure(new IntPtr(handle.ToInt64() + OfsCommon), typeof (KLstDevCommonInfo));

        #region IKLIB_HANDLE Members
        public KlibHandleType HandleType => KlibHandleType.LstInfok;

        public IntPtr GetContext()
        {
            return NativeMethods.LibK_GetContext(handle, HandleType);
        }

        public bool SetContext(IntPtr contextValue)
        {
            return NativeMethods.LibK_SetContext(handle, HandleType, contextValue);
        }

        public IntPtr DangerousGetHandle()
        {
            return handle;
        }

        public bool SetCleanupCallback(KLibHandleCleanupCb CleanupCallback)
        {
            if (CleanupCallback == null)
            {
                throw new ArgumentNullException(nameof(CleanupCallback));
            }

            return NativeMethods.LibK_SetCleanupCallback(handle, HandleType, CleanupCallback);
        }
        #endregion

        public override string ToString()
        {
            return
                $"{{Common: {Common}, DriverID: {DriverId}, DeviceInterfaceGuid: {DeviceInterfaceGuid}, InstanceId: {InstanceId}, ClassGuid: {ClassGuid}, Mfg: {Mfg}, DeviceDesc: {DeviceDesc}, Service: {Service}, SymbolicLink: {SymbolicLink}, DevicePath: {DevicePath}, LUsb0FilterIndex: {LUsb0FilterIndex}, Connected: {Connected}, SyncFlags: {SyncFlags}}}";
        }
    }
    #endregion

    #region Enumerations
    /// <Summary>Values used in the \c bmAttributes field of a \ref USB_ENDPOINT_DESCRIPTOR</Summary>
    public enum UsbdPipeType
    {
        /// <Summary>Indicates a control endpoint</Summary>
        UsbdPipeTypeControl,

        /// <Summary>Indicates an isochronous endpoint</Summary>
        UsbdPipeTypeIsochronous,

        /// <Summary>Indicates a bulk endpoint</Summary>
        UsbdPipeTypeBulk,

        /// <Summary>Indicates an interrupt endpoint</Summary>
        UsbdPipeTypeInterrupt,
    }

    /// <Summary>Additional ISO transfer flags.</Summary>
    [Flags]
    public enum KisoFlag
    {
        None = 0,

        /// <Summary>Do not start the transfer immediately, instead use \ref KISO_CONTEXT::StartFrame.</Summary>
        SetStartFrame = 0x00000001,
    }

    /// <Summary>Handle type enumeration.</Summary>
    public enum KlibHandleType
    {
        /// <Summary>Hot plug handle. \ref KHOT_HANDLE</Summary>
        HotK,

        /// <Summary>USB handle. \ref KUSB_HANDLE</Summary>
        UsbK,

        /// <Summary>Shared USB handle. \ref KUSB_HANDLE</Summary>
        UsbSharedK,

        /// <Summary>Device list handle. \ref KLST_HANDLE</Summary>
        LstK,

        /// <Summary>Device info handle. \ref KLST_DEVINFO_HANDLE</Summary>
        LstInfok,

        /// <Summary>Overlapped handle. \ref KOVL_HANDLE</Summary>
        OvlK,

        /// <Summary>Overlapped pool handle. \ref KOVL_POOL_HANDLE</Summary>
        OvlPoolK,

        /// <Summary>Pipe stream handle. \ref KSTM_HANDLE</Summary>
        StmK,

        /// <Summary>Max handle type count.</Summary>
        Count
    }

    /// <Summary>Device list sync flags.</Summary>
    [Flags]
    public enum KlstSyncFlag
    {
        /// <Summary>Cleared/invalid state.</Summary>
        None = 0,

        /// <Summary>Unchanged state,</Summary>
        Unchanged = 0x0001,

        /// <Summary>Added (Arrival) state,</Summary>
        Added = 0x0002,

        /// <Summary>Removed (Unplugged) state,</Summary>
        Removed = 0x0004,

        /// <Summary>Connect changed state.</Summary>
        ConnectChange = 0x0008,

        /// <Summary>All states.</Summary>
        Mask = 0x000F,
    }

    /// <Summary>Device list initialization flags.</Summary>
    [Flags]
    public enum KlstFlag
    {
        /// <Summary>No flags (or 0)</Summary>
        None = 0,

        /// <Summary>Enable listings for the raw device interface GUID.{A5DCBF10-6530-11D2-901F-00C04FB951ED}</Summary>
        IncludeRawGuid = 0x0001,

        /// <Summary>List libusbK devices that not currently connected.</Summary>
        IncludeDisconnect = 0x0002,
    }

    /// <Summary>bmRequest.Dir</Summary>
    public enum BmRequestDir
    {
        HostToDevice = 0,
        DeviceToHost = 1,
    }

    /// <Summary>bmRequest.Type</Summary>
    public enum BmRequestType
    {
        /// <Summary>Standard request. See \ref USB_REQUEST_ENUM</Summary>
        Standard = 0,

        /// <Summary>Class-specific request.</Summary>
        Class = 1,

        /// <Summary>Vendor-specific request</Summary>
        Vendor = 2,
    }

    /// <Summary>bmRequest.Recipient</Summary>
    public enum BmRequestRecipient
    {
        /// <Summary>Request is for a device.</Summary>
        Device = 0,

        /// <Summary>Request is for an interface of a device.</Summary>
        Interface = 1,

        /// <Summary>Request is for an endpoint of a device.</Summary>
        Endpoint = 2,

        /// <Summary>Request is for a vendor-specific purpose.</Summary>
        Other = 3,
    }

    /// <Summary>Values for the bits returned by the \ref USB_REQUEST_GET_STATUS request.</Summary>
    public enum UsbGetStatus
    {
        /// <Summary>Device is self powered</Summary>
        SelfPowered = 0x01,

        /// <Summary>Device can wake the system from a low power/sleeping state.</Summary>
        RemoteWakeupEnabled = 0x02
    }

    /// <Summary>Standard USB descriptor types. For more information, see section 9-5 of the USB 3.0 specifications.</Summary>
    public enum UsbDescriptorType
    {
        /// <Summary>Device descriptor type.</Summary>
        Device = 0x01,

        /// <Summary>Configuration descriptor type.</Summary>
        Configuration = 0x02,

        /// <Summary>String descriptor type.</Summary>
        String = 0x03,

        /// <Summary>Interface descriptor type.</Summary>
        Interface = 0x04,

        /// <Summary>Endpoint descriptor type.</Summary>
        Endpoint = 0x05,

        /// <Summary>Device qualifier descriptor type.</Summary>
        DeviceQualifier = 0x06,

        /// <Summary>Config power descriptor type.</Summary>
        ConfigPower = 0x07,

        /// <Summary>Interface power descriptor type.</Summary>
        InterfacePower = 0x08,

        /// <Summary>Interface association descriptor type.</Summary>
        InterfaceAssociation = 0x0B,

        /// <summary>An enum constant representing the dfu functional descriptor.</summary>
        DfuFunctional = 0x21,
    }

    //! USB defined request codes
    /*
    * see Chapter 9 of the USB 2.0 specification for
    * more information.
    *
    * These are the correct values based on the USB 2.0 specification.
    */
    public enum UsbRequestEnum
    {
        //! Request status of the specific recipient
        UsbRequestGetStatus = 0x00,

        //! Clear or disable a specific feature
        UsbRequestClearFeature = 0x01,

        //! Set or enable a specific feature
        UsbRequestSetFeature = 0x03,

        //! Set device address for all future accesses
        UsbRequestSetAddress = 0x05,

        //! Get the specified descriptor
        UsbRequestGetDescriptor = 0x06,

        //! Update existing descriptors or add new descriptors
        UsbRequestSetDescriptor = 0x07,

        //! Get the current device configuration value
        UsbRequestGetConfiguration = 0x08,

        //! Set device configuration
        UsbRequestSetConfiguration = 0x09,

        //! Return the selected alternate setting for the specified interface
        UsbRequestGetInterface = 0x0A,

        //! Select an alternate interface for the specified interface
        UsbRequestSetInterface = 0x0B,

        //! Set then report an endpoint's synchronization frame
        UsbRequestSyncFrame = 0x0C,
    };

    /// <Summary>Usb handle specific properties that can be retrieved with \ref UsbK_GetProperty.</Summary>
    public enum KUsbProperty
    {
        /// <Summary>Get the internal device file handle used for operations such as GetOverlappedResult or DeviceIoControl.</Summary>
        DeviceFileHandle,

        Count
    }

    /// <Summary>Supported driver id enumeration.</Summary>
    public enum KUsbDrvId
    {
        /// <Summary>libusbK.sys driver ID</Summary>
        LibUsbK,

        /// <Summary>libusb0.sys driver ID</Summary>
        LibUsb0,

        /// <Summary>WinUSB.sys driver ID</Summary>
        WinUsb,

        /// <Summary>libusb0.sys filter driver ID</Summary>
        LibUsb0Filter,

        /// <Summary>Supported driver count</Summary>
        Count
    }

    /// <Summary>Supported function id enumeration.</Summary>
    public enum KUsbFnId
    {
        /// <Summary>\ref UsbK_Init dynamic driver function id.</Summary>
        Init,

        /// <Summary>\ref UsbK_Free dynamic driver function id.</Summary>
        Free,

        /// <Summary>\ref UsbK_ClaimInterface dynamic driver function id.</Summary>
        ClaimInterface,

        /// <Summary>\ref UsbK_ReleaseInterface dynamic driver function id.</Summary>
        ReleaseInterface,

        /// <Summary>\ref UsbK_SetAltInterface dynamic driver function id.</Summary>
        SetAltInterface,

        /// <Summary>\ref UsbK_GetAltInterface dynamic driver function id.</Summary>
        GetAltInterface,

        /// <Summary>\ref UsbK_GetDescriptor dynamic driver function id.</Summary>
        GetDescriptor,

        /// <Summary>\ref UsbK_ControlTransfer dynamic driver function id.</Summary>
        ControlTransfer,

        /// <Summary>\ref UsbK_SetPowerPolicy dynamic driver function id.</Summary>
        SetPowerPolicy,

        /// <Summary>\ref UsbK_GetPowerPolicy dynamic driver function id.</Summary>
        GetPowerPolicy,

        /// <Summary>\ref UsbK_SetConfiguration dynamic driver function id.</Summary>
        SetConfiguration,

        /// <Summary>\ref UsbK_GetConfiguration dynamic driver function id.</Summary>
        GetConfiguration,

        /// <Summary>\ref UsbK_ResetDevice dynamic driver function id.</Summary>
        ResetDevice,

        /// <Summary>\ref UsbK_Initialize dynamic driver function id.</Summary>
        Initialize,

        /// <Summary>\ref UsbK_SelectInterface dynamic driver function id.</Summary>
        SelectInterface,

        /// <Summary>\ref UsbK_GetAssociatedInterface dynamic driver function id.</Summary>
        GetAssociatedInterface,

        /// <Summary>\ref UsbK_Clone dynamic driver function id.</Summary>
        Clone,

        /// <Summary>\ref UsbK_QueryInterfaceSettings dynamic driver function id.</Summary>
        QueryInterfaceSettings,

        /// <Summary>\ref UsbK_QueryDeviceInformation dynamic driver function id.</Summary>
        QueryDeviceInformation,

        /// <Summary>\ref UsbK_SetCurrentAlternateSetting dynamic driver function id.</Summary>
        SetCurrentAlternateSetting,

        /// <Summary>\ref UsbK_GetCurrentAlternateSetting dynamic driver function id.</Summary>
        GetCurrentAlternateSetting,

        /// <Summary>\ref UsbK_QueryPipe dynamic driver function id.</Summary>
        QueryPipe,

        /// <Summary>\ref UsbK_SetPipePolicy dynamic driver function id.</Summary>
        SetPipePolicy,

        /// <Summary>\ref UsbK_GetPipePolicy dynamic driver function id.</Summary>
        GetPipePolicy,

        /// <Summary>\ref UsbK_ReadPipe dynamic driver function id.</Summary>
        ReadPipe,

        /// <Summary>\ref UsbK_WritePipe dynamic driver function id.</Summary>
        WritePipe,

        /// <Summary>\ref UsbK_ResetPipe dynamic driver function id.</Summary>
        ResetPipe,

        /// <Summary>\ref UsbK_AbortPipe dynamic driver function id.</Summary>
        AbortPipe,

        /// <Summary>\ref UsbK_FlushPipe dynamic driver function id.</Summary>
        FlushPipe,

        /// <Summary>\ref UsbK_IsoReadPipe dynamic driver function id.</Summary>
        IsoReadPipe,

        /// <Summary>\ref UsbK_IsoWritePipe dynamic driver function id.</Summary>
        IsoWritePipe,

        /// <Summary>\ref UsbK_GetCurrentFrameNumber dynamic driver function id.</Summary>
        GetCurrentFrameNumber,

        /// <Summary>\ref UsbK_GetOverlappedResult dynamic driver function id.</Summary>
        GetOverlappedResult,

        /// <Summary>\ref UsbK_GetProperty dynamic driver function id.</Summary>
        GetProperty,

        /// <Summary>Supported function count</Summary>
        Count,
    }

    /// <Summary>Hot plug config flags.</Summary>
    [Flags]
    public enum KHotFlag
    {
        /// <Summary>No flags (or 0)</Summary>
        None,

        /// <Summary>Notify all devices which match upon a successful call to \ref HotK_Init.</Summary>
        PlugAllOnInit = 0x0001,

        /// <Summary>Allow other \ref KHOT_HANDLE instances to consume this match.</Summary>
        PassDupeInstance = 0x0002,

        /// <Summary>If a \c UserHwnd is specified, use \c PostMessage instead of \c SendMessage.</Summary>
        PostUserMessage = 0x0004,
    }

    /// <Summary>\c WaitFlags used by \ref OvlK_Wait.</Summary>
    [Flags]
    public enum KOvlWaitFlag
    {
        /// <Summary>Do not perform any additional actions upon exiting \ref OvlK_Wait.</Summary>
        None = 0,

        /// <Summary>If the i/o operation completes successfully, release the OverlappedK back to it's pool.</Summary>
        ReleaseOnSuccess = 0x0001,

        /// <Summary>If the i/o operation fails, release the OverlappedK back to it's pool.</Summary>
        ReleaseOnFail = 0x0002,

        /// <Summary>If the i/o operation fails or completes successfully, release the OverlappedK back to its pool. Perform no actions if it times-out.</Summary>
        ReleaseOnSuccessFail = 0x0003,

        /// <Summary>If the i/o operation times-out cancel it, but do not release the OverlappedK back to its pool.</Summary>
        CancelOnTimeout = 0x0004,

        /// <Summary>If the i/o operation times-out, cancel it and release the OverlappedK back to its pool.</Summary>
        ReleaseOnTimeout = 0x000C,

        /// <Summary>Always release the OverlappedK back to its pool.  If the operation timed-out, cancel it before releasing back to its pool.</Summary>
        ReleaseAlways = 0x000F,

        /// <Summary>Uses alterable wait functions.  See http://msdn.microsoft.com/en-us/library/windows/desktop/ms687036%28v=vs.85%29.aspx</Summary>
        Alertable = 0x0010,
    }

    /// <Summary>\c Overlapped pool config flags.</Summary>
    [Flags]
    public enum KOvlPoolFlag
    {
        None = 0,
    }

    /// <Summary>Stream config flags.</Summary>
    [Flags]
    public enum KStmFlag
    {
        None = 0,
    }

    /// <Summary>Stream config flags.</Summary>
    public enum KStmCompleteResult
    {
        Valid = 0,
        Invalid,
    }
    #endregion

    #region Structs
    /// <Summary>The \c WINUSB_PIPE_INFORMATION structure contains pipe information that the \ref UsbK_QueryPipe routine retrieves.</Summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct WinUsbPipeInformation
    {
        /// <Summary>A \c USBD_PIPE_TYPE enumeration value that specifies the pipe type</Summary>
        public UsbdPipeType PipeType;

        /// <Summary>The pipe identifier (ID)</Summary>
        public byte PipeId;

        /// <Summary>The maximum size, in bytes, of the packets that are transmitted on the pipe</Summary>
        public ushort MaximumPacketSize;

        /// <Summary>The pipe interval</Summary>
        public byte Interval;
    };

    /// <Summary>The \c WINUSB_SETUP_PACKET structure describes a USB setup packet.</Summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct WinUsbSetupPacket
    {
        /// <Summary>The request type. The values that are assigned to this member are defined in Table 9.2 of section 9.3 of the Universal Serial Bus (USB) specification (www.usb.org).</Summary>
        public byte RequestType;

        /// <Summary>The device request. The values that are assigned to this member are defined in Table 9.3 of section 9.4 of the Universal Serial Bus (USB) specification.</Summary>
        public byte Request;

        /// <Summary>The meaning of this member varies according to the request. For an explanation of this member, see the Universal Serial Bus (USB) specification.</Summary>
        public ushort Value;

        /// <Summary>The meaning of this member varies according to the request. For an explanation of this member, see the Universal Serial Bus (USB) specification.</Summary>
        public ushort Index;

        /// <Summary>The number of bytes to transfer. (not including the \c WINUSB_SETUP_PACKET itself)</Summary>
        public ushort Length;
    };

    /// <Summary>Structure describing an isochronous transfer packet.</Summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct KIsoPacket
    {
        /// <Summary>Specifies the offset, in bytes, of the buffer for this packet from the beginning of the entire isochronous transfer data buffer.</Summary>
        public uint Offset;

        /// <Summary>Set by the host controller to indicate the actual number of bytes received by the device for isochronous IN transfers. Length not used for isochronous OUT transfers.</Summary>
        public ushort Length;

        /// <Summary>Contains the 16 least significant USBD status bits, on return from the host controller driver, of this transfer packet.</Summary>
        public ushort Status;
    };

    /// <summary>
    ///   KISO_CONTEXT_MAP is used for calculating field offsets only as it lacks an IsoPackets field.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 16)]
    internal struct KIsoContextMap
    {
        /// <Summary>Additional ISO transfer flags. See \ref KISO_FLAG.</Summary>
        public KisoFlag Flags;

        /// <Summary>Specifies the frame number that the transfer should begin on (0 for ASAP).</Summary>
        public uint StartFrame;

        /// <Summary>Contains the number of packets that completed with an error condition on return from the host controller driver.</Summary>
        public short ErrorCount;

        /// <Summary>Specifies the number of packets that are described by the variable-length array member \c IsoPacket.</Summary>
        public short NumberOfPackets;
    };

    /// <Summary>libusbK verson information structure.</Summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct KLibVersion
    {
        /// <Summary>Major version number.</Summary>
        public int Major;

        /// <Summary>Minor version number.</Summary>
        public int Minor;

        /// <Summary>Micro version number.</Summary>
        public int Micro;

        /// <Summary>Nano version number.</Summary>
        public int Nano;
    };

    /// <Summary>Common usb device information structure</Summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct KLstDevCommonInfo
    {
        /// <Summary>VendorID parsed from \ref KLST_DEVINFO::InstanceID</Summary>
        public int Vid;

        /// <Summary>ProductID parsed from \ref KLST_DEVINFO::InstanceID</Summary>
        public int Pid;

        /// <Summary>Interface number (valid for composite devices only) parsed from \ref KLST_DEVINFO::InstanceID</Summary>
        public int MI;

        // An ID that uniquely identifies a USB device.
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.KlstStringMaxLen)]
        public string InstanceID;
    };

    /// <Summary>Semi-opaque device information structure of a device list.</Summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct KLstDevinfo
    {
        /// <Summary>Common usb device information</Summary>
        public KLstDevCommonInfo Common;

        /// <Summary>Driver id this device element is using</Summary>
        public int DriverID;

        /// <Summary>Device interface GUID</Summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.KlstStringMaxLen)]
        public string DeviceInterfaceGUID;

        /// <Summary>Device instance ID.</Summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.KlstStringMaxLen)]
        public string InstanceID;

        /// <Summary>Class GUID.</Summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.KlstStringMaxLen)]
        public string ClassGUID;

        /// <Summary>Manufacturer name as specified in the INF file.</Summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.KlstStringMaxLen)]
        public string Mfg;

        /// <Summary>Device description as specified in the INF file.</Summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.KlstStringMaxLen)]
        public string DeviceDesc;

        /// <Summary>Driver service name.</Summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.KlstStringMaxLen)]
        public string Service;

        /// <Summary>Unique identifier.</Summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.KlstStringMaxLen)]
        public string SymbolicLink;

        /// <Summary>physical device filename used with the Windows \c CreateFile()</Summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.KlstStringMaxLen)]
        public string DevicePath;

        /// <Summary>libusb-win32 filter index id.</Summary>
        public int LUsb0FilterIndex;

        /// <Summary>Indicates the devices connection state.</Summary>
        public bool Connected;

        /// <Summary>Synchronization flags. (internal use only)</Summary>
        public KlstSyncFlag SyncFlags;
    };

    /// <Summary>Hot plug parameter structure.</Summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 1024)]
    public struct KLstPatternMatch
    {
        /// <Summary>Pattern match a device instance id.</Summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.KlstStringMaxLen)]
        public string InstanceID;

        /// <Summary>Pattern match a device interface guid.</Summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.KlstStringMaxLen)]
        public string DeviceInterfaceGUID;

        /// <Summary>Pattern match a symbolic link.</Summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.KlstStringMaxLen)]
        public string SymbolicLink;
    };

    /// <Summary>A structure representing the standard USB device descriptor.</Summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct UsbDeviceDescriptor
    {
        /// <Summary>Size of this descriptor (in bytes)</Summary>
        public byte bLength;

        /// <Summary>Descriptor type</Summary>
        public byte bDescriptorType;

        /// <Summary>USB specification release number in binary-coded decimal.</Summary>
        public ushort bcdUSB;

        /// <Summary>USB-IF class code for the device</Summary>
        public byte bDeviceClass;

        /// <Summary>USB-IF subclass code for the device</Summary>
        public byte bDeviceSubClass;

        /// <Summary>USB-IF protocol code for the device</Summary>
        public byte bDeviceProtocol;

        /// <Summary>Maximum packet size for control endpoint 0</Summary>
        public byte bMaxPacketSize0;

        /// <Summary>USB-IF vendor ID</Summary>
        public ushort idVendor;

        /// <Summary>USB-IF product ID</Summary>
        public ushort idProduct;

        /// <Summary>Device release number in binary-coded decimal</Summary>
        public ushort bcdDevice;

        /// <Summary>Index of string descriptor describing manufacturer</Summary>
        public byte iManufacturer;

        /// <Summary>Index of string descriptor describing product</Summary>
        public byte iProduct;

        /// <Summary>Index of string descriptor containing device serial number</Summary>
        public byte iSerialNumber;

        /// <Summary>Number of possible configurations</Summary>
        public byte bNumConfigurations;
    };

    /// <Summary>A structure representing the standard USB endpoint descriptor.</Summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct UsbEndpointDescriptor
    {
        /// <Summary>Size of this descriptor (in bytes)</Summary>
        public byte bLength;

        /// <Summary>Descriptor type</Summary>
        public byte bDescriptorType;

        /// <Summary>The address of the endpoint described by this descriptor.</Summary>
        public byte bEndpointAddress;

        /// <Summary>Attributes which apply to the endpoint when it is configured using the bConfigurationValue.</Summary>
        public byte bmAttributes;

        /// <Summary>Maximum packet size this endpoint is capable of sending/receiving.</Summary>
        public ushort wMaxPacketSize;

        /// <Summary>Interval for polling endpoint for data transfers.</Summary>
        public byte bInterval;
    };

    /// <Summary>A structure representing the standard USB configuration descriptor.</Summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct UsbConfigurationDescriptor
    {
        /// <Summary>Size of this descriptor (in bytes)</Summary>
        public byte bLength;

        /// <Summary>Descriptor type</Summary>
        public byte bDescriptorType;

        /// <Summary>Total length of data returned for this configuration</Summary>
        public ushort wTotalLength;

        /// <Summary>Number of interfaces supported by this configuration</Summary>
        public byte bNumInterfaces;

        /// <Summary>Identifier value for this configuration</Summary>
        public byte bConfigurationValue;

        /// <Summary>Index of string descriptor describing this configuration</Summary>
        public byte iConfiguration;

        /// <Summary>Configuration characteristics</Summary>
        public byte bmAttributes;

        /// <Summary>Maximum power consumption of the USB device from this bus in this configuration when the device is fully operation.</Summary>
        public byte MaxPower;
    };

    /// <Summary>A structure representing the standard USB interface descriptor.</Summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct UsbInterfaceDescriptor
    {
        /// <Summary>Size of this descriptor (in bytes)</Summary>
        public byte bLength;

        /// <Summary>Descriptor type</Summary>
        public byte bDescriptorType;

        /// <Summary>Number of this interface</Summary>
        public byte bInterfaceNumber;

        /// <Summary>Value used to select this alternate setting for this interface</Summary>
        public byte bAlternateSetting;

        /// <Summary>Number of endpoints used by this interface (excluding the control endpoint)</Summary>
        public byte bNumEndpoints;

        /// <Summary>USB-IF class code for this interface</Summary>
        public byte bInterfaceClass;

        /// <Summary>USB-IF subclass code for this interface</Summary>
        public byte bInterfaceSubClass;

        /// <Summary>USB-IF protocol code for this interface</Summary>
        public byte bInterfaceProtocol;

        /// <Summary>Index of string descriptor describing this interface</Summary>
        public byte iInterface;
    };

    /// <Summary>A structure representing the standard USB string descriptor.</Summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
    public struct UsbStringDescriptor
    {
        /// <Summary>Size of this descriptor (in bytes)</Summary>
        public byte bLength;

        /// <Summary>Descriptor type</Summary>
        public byte bDescriptorType;

        /// <Summary>Content of the string</Summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.KlstStringMaxLen)]
        public string bString;
    };

    /// <Summary>A structure representing the common USB descriptor.</Summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct UsbCommonDescriptor
    {
        /// <Summary>Size of this descriptor (in bytes)</Summary>
        public byte bLength;

        /// <Summary>Descriptor type</Summary>
        public byte bDescriptorType;
    };

    /// <Summary>Allows hardware manufacturers to define groupings of interfaces.</Summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct UsbInterfaceAssociationDescriptor
    {
        /// <Summary>Size of this descriptor (in bytes)</Summary>
        public byte bLength;

        /// <Summary>Descriptor type</Summary>
        public byte bDescriptorType;

        /// <Summary>First interface number of the set of interfaces that follow this descriptor</Summary>
        public byte bFirstInterface;

        /// <Summary>The Number of interfaces follow this descriptor that are considered "associated"</Summary>
        public byte bInterfaceCount;

        /// <Summary>\c bInterfaceClass used for this associated interfaces</Summary>
        public byte bFunctionClass;

        /// <Summary>\c bInterfaceSubClass used for the associated interfaces</Summary>
        public byte bFunctionSubClass;

        /// <Summary>\c bInterfaceProtocol used for the associated interfaces</Summary>
        public byte bFunctionProtocol;

        /// <Summary>Index of string descriptor describing the associated interfaces</Summary>
        public byte iFunction;
    };

    /// <summary>
    /// DFU functional descriptor
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct UsbDfuFunctionalDescriptor
    {
        /// <Summary>Size of this descriptor (in bytes)</Summary>
        public byte bLength;

        /// <Summary>Descriptor type.</Summary>
        public byte bDescriptorType;

        /// <Summary>Attributes.</Summary>
        public byte bmAttributes;

        /// <summary>Detach timeout.</summary>
        public ushort wDetachTimeOut;

        /// <summary>Transfer size for download/upload.</summary>
        public ushort wTransferSize;

        /// <summary>Binary coded(BCD) value of DFU version.</summary>
        public ushort bcdDFUVersion;
    };

    /// <Summary>USB core driver API information structure.</Summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct KUsbDriverApiInfo
    {
        /// <Summary>readonly Driver id of the driver api.</Summary>
        public int DriverID;

        /// <Summary>readonly Number of valid functions contained in the driver API.</Summary>
        public int FunctionCount;
    };

    /// <Summary>Driver API function set structure.</Summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 512)]
    public struct KUsbDriverApi
    {
        /// <Summary>Driver API information.</Summary>
        public KUsbDriverApiInfo Info;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbInitDelegate Init;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbFreeDelegate Free;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbClaimInterfaceDelegate ClaimInterface;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbReleaseInterfaceDelegate ReleaseInterface;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbSetAltInterfaceDelegate SetAltInterface;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbGetAltInterfaceDelegate GetAltInterface;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbGetDescriptorDelegate GetDescriptor;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbControlTransferDelegate ControlTransfer;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbSetPowerPolicyDelegate SetPowerPolicy;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbGetPowerPolicyDelegate GetPowerPolicy;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbSetConfigurationDelegate SetConfiguration;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbGetConfigurationDelegate GetConfiguration;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbResetDeviceDelegate ResetDevice;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbInitializeDelegate Initialize;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbSelectInterfaceDelegate SelectInterface;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbGetAssociatedInterfaceDelegate GetAssociatedInterface;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbCloneDelegate Clone;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbQueryInterfaceSettingsDelegate QueryInterfaceSettings;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbQueryDeviceInformationDelegate QueryDeviceInformation;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbSetCurrentAlternateSettingDelegate SetCurrentAlternateSetting;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbGetCurrentAlternateSettingDelegate GetCurrentAlternateSetting;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbQueryPipeDelegate QueryPipe;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbSetPipePolicyDelegate SetPipePolicy;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbGetPipePolicyDelegate GetPipePolicy;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbReadPipeDelegate ReadPipe;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbWritePipeDelegate WritePipe;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbResetPipeDelegate ResetPipe;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbAbortPipeDelegate AbortPipe;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbFlushPipeDelegate FlushPipe;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbIsoReadPipeDelegate IsoReadPipe;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbIsoWritePipeDelegate IsoWritePipe;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbGetCurrentFrameNumberDelegate GetCurrentFrameNumber;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbGetOverlappedResultDelegate GetOverlappedResult;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KUsbGetPropertyDelegate GetProperty;
    };

    /// <Summary>Hot plug parameter structure.</Summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 2048)]
    public struct KHotParams
    {
        /// <Summary>Hot plug event window handle to send/post messages when notifications occur.</Summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Justification = "Required by libusbk")]
        public IntPtr UserHwnd;

        /// <Summary>WM_USER message start offset used when sending/posting messages, See details.</Summary>
        public uint UserMessage;

        /// <Summary>Additional init/config parameters</Summary>
        public KHotFlag Flags;

        /// <Summary>File pattern matches for restricting notifications to a single/group or all supported usb devices.</Summary>
        public KLstPatternMatch PatternMatch;

        /// <Summary>Hot plug event callback function invoked when notifications occur.</Summary>
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KHotPlugCb OnHotPlug;
    };

    /// <Summary>Stream transfer context structure.</Summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct KStmXferContext
    {
        /// <Summary>Internal stream buffer.</Summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Justification = "Required by libusbk")]
        public IntPtr Buffer;

        /// <Summary>Size of internal stream buffer.</Summary>
        public int BufferSize;

        /// <Summary>Number of bytes to write or number of bytes read.</Summary>
        public int TransferLength;

        /// <Summary>User defined state.</Summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Justification = "Required by libusbk")]
        public IntPtr UserState;
    };

    /// <Summary>Stream information structure.</Summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct KStmInfo
    {
        /// <Summary>\ref KUSB_HANDLE this stream uses.</Summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Justification = "Required by libusbk")]
        public IntPtr UsbHandle;

        /// <Summary>This parameter corresponds to the bEndpointAddress field in the endpoint descriptor.</Summary>
        public byte PipeID;

        /// <Summary>Maximum transfer read/write request allowed pending.</Summary>
        public int MaxPendingTransfers;

        /// <Summary>Maximum transfer sage size.</Summary>
        public int MaxTransferSize;

        /// <Summary>Maximum number of I/O request allowed pending.</Summary>
        public int MaxPendingIO;

        /// <Summary>Populated with the endpoint descriptor for the specified \c PipeID.</Summary>
        public UsbEndpointDescriptor EndpointDescriptor;

        /// <Summary>Populated with the driver api for the specified \c UsbHandle.</Summary>
        public KUsbDriverApi DriverAPI;

        /// <Summary>Populated with the device file handle for the specified \c UsbHandle.</Summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Justification = "Required by libusbk")]
        public IntPtr DeviceHandle;
    };

    /// <Summary>Stream callback structure.</Summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 64)]
    public struct KStmCallback
    {
        /// <Summary>Executed when a transfer error occurs.</Summary>
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KStmErrorCb Error;

        /// <Summary>Executed to submit a transfer.</Summary>
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KStmSubmitCb Submit;

        /// <Summary>Executed when a valid transfer completes.</Summary>
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KStmCompleteCb Complete;

        /// <Summary>Executed for every transfer context when the stream is started with \ref StmK_Start.</Summary>
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KStmStartedCb Started;

        /// <Summary>Executed for every transfer context when the stream is stopped with \ref StmK_Stop.</Summary>
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KStmStoppedCb Stopped;

        /// <Summary>Executed immediately after a transfer completes.</Summary>
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public KStmBeforeCompleteCb BeforeComplete;
    };
    #endregion

    #region Delegates
    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate int KLibHandleCleanupCb([In] IntPtr handle, KlibHandleType handleType, IntPtr userContext);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KIsoEnumPacketsCb(uint packetIndex, [In] ref KIsoPacket isoPacket, IntPtr userState);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KLstEnumDevInfoCb([In] IntPtr deviceList, [In] KLstDevInfoHandle deviceInfo, IntPtr context);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbInitDelegate([Out] out KusbHandle interfaceHandle, [In] KLstDevInfoHandle devInfo);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbFreeDelegate([In] KusbHandle interfaceHandle);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbClaimInterfaceDelegate([In] KusbHandle interfaceHandle, byte numberOrIndex, bool isIndex);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbReleaseInterfaceDelegate([In] KusbHandle interfaceHandle, byte numberOrIndex, bool isIndex);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbSetAltInterfaceDelegate([In] KusbHandle interfaceHandle, byte numberOrIndex, bool isIndex, byte altSettingNumber);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbGetAltInterfaceDelegate([In] KusbHandle interfaceHandle, byte numberOrIndex, bool isIndex, out byte altSettingNumber);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbGetDescriptorDelegate(
        [In] KusbHandle interfaceHandle, byte descriptorType, byte index, ushort languageId, IntPtr buffer, uint bufferLength, out uint lengthTransferred);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbControlTransferDelegate(
        [In] KusbHandle interfaceHandle, WinUsbSetupPacket setupPacket, IntPtr buffer, uint bufferLength, out uint lengthTransferred, IntPtr overlapped);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbSetPowerPolicyDelegate([In] KusbHandle interfaceHandle, uint policyType, uint valueLength, IntPtr value);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbGetPowerPolicyDelegate([In] KusbHandle interfaceHandle, uint policyType, ref uint valueLength, IntPtr value);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbSetConfigurationDelegate([In] KusbHandle interfaceHandle, byte configurationNumber);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbGetConfigurationDelegate([In] KusbHandle interfaceHandle, out byte configurationNumber);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbResetDeviceDelegate([In] KusbHandle interfaceHandle);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbInitializeDelegate(IntPtr deviceHandle, [Out] out KusbHandle interfaceHandle);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbSelectInterfaceDelegate([In] KusbHandle interfaceHandle, byte numberOrIndex, bool isIndex);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbGetAssociatedInterfaceDelegate(
        [In] KusbHandle interfaceHandle, byte associatedInterfaceIndex, [Out] out KusbHandle associatedInterfaceHandle);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbCloneDelegate([In] KusbHandle interfaceHandle, [Out] out KusbHandle dstInterfaceHandle);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbQueryInterfaceSettingsDelegate(
        [In] KusbHandle interfaceHandle, byte altSettingNumber, [Out] out UsbInterfaceDescriptor usbAltInterfaceDescriptor);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbQueryDeviceInformationDelegate([In] KusbHandle interfaceHandle, uint informationType, ref uint bufferLength, IntPtr buffer);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbSetCurrentAlternateSettingDelegate([In] KusbHandle interfaceHandle, byte altSettingNumber);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbGetCurrentAlternateSettingDelegate([In] KusbHandle interfaceHandle, out byte altSettingNumber);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbQueryPipeDelegate([In] KusbHandle interfaceHandle, byte altSettingNumber, byte pipeIndex, [Out] out WinUsbPipeInformation pipeInformation);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbSetPipePolicyDelegate([In] KusbHandle interfaceHandle, byte pipeId, uint policyType, uint valueLength, IntPtr value);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbGetPipePolicyDelegate([In] KusbHandle interfaceHandle, byte pipeId, uint policyType, ref uint valueLength, IntPtr value);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbReadPipeDelegate(
        [In] KusbHandle interfaceHandle, byte pipeId, IntPtr buffer, uint bufferLength, out uint lengthTransferred, IntPtr overlapped);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbWritePipeDelegate(
        [In] KusbHandle interfaceHandle, byte pipeId, IntPtr buffer, uint bufferLength, out uint lengthTransferred, IntPtr overlapped);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbResetPipeDelegate([In] KusbHandle interfaceHandle, byte pipeId);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbAbortPipeDelegate([In] KusbHandle interfaceHandle, byte pipeId);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbFlushPipeDelegate([In] KusbHandle interfaceHandle, byte pipeId);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbIsoReadPipeDelegate(
        [In] KusbHandle interfaceHandle, byte pipeId, IntPtr buffer, uint bufferLength, IntPtr overlapped, [In] KisoContext isoContext);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbIsoWritePipeDelegate(
        [In] KusbHandle interfaceHandle, byte pipeId, IntPtr buffer, uint bufferLength, IntPtr overlapped, [In] KisoContext isoContext);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbGetCurrentFrameNumberDelegate([In] KusbHandle interfaceHandle, out uint frameNumber);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbGetOverlappedResultDelegate([In] KusbHandle interfaceHandle, IntPtr overlapped, out uint lpNumberOfBytesTransferred, bool bWait);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate bool KUsbGetPropertyDelegate([In] KusbHandle interfaceHandle, KUsbProperty propertyType, ref uint propertySize, IntPtr value);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate void KHotPlugCb([In] IntPtr hotHandle, [In] KLstDevInfoHandle deviceInfo, KlstSyncFlag plugType);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate int KStmErrorCb([In] ref KStmInfo streamInfo, [In] ref KStmXferContext transferContext, int errorCode);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate int KStmSubmitCb([In] ref KStmInfo streamInfo, [In] ref KStmXferContext transferContext, IntPtr overlapped);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate int KStmStartedCb([In] ref KStmInfo streamInfo, [In] ref KStmXferContext transferContext, int transferContextIndex);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate int KStmStoppedCb([In] ref KStmInfo streamInfo, [In] ref KStmXferContext transferContext, int transferContextIndex);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate int KStmCompleteCb([In] ref KStmInfo streamInfo, [In] ref KStmXferContext transferContext, int errorCode);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi, SetLastError = true)]
    public delegate KStmCompleteResult KStmBeforeCompleteCb([In] ref KStmInfo streamInfo, [In] ref KStmXferContext transferContext, ref int errorCode);
    #endregion


    public class LstK
    {
        protected readonly KlstHandle handle;

        /// <Summary>Initializes a new usb device list containing all supported devices.</Summary>
        public LstK(KlstFlag flags)
        {
            RuntimeHelpers.PrepareConstrainedRegions();

            try
            {
            }
            finally
            {
                bool success = NativeMethods.LstK_Init(out handle, flags);
                int errorCode = Marshal.GetLastWin32Error();

                if (!success || handle.IsInvalid || handle.IsClosed)
                {
                    handle.SetHandleAsInvalid();
                    throw new Exception(GetType().Name + " failed. ErrorCode=" + errorCode.ToString("X"));
                }

                uint count = 0;
                NativeMethods.LstK_Count(handle, ref count);
                if (count <= 0)
                {
                    throw new Exception("No Devices Connected.");
                }
            }
        }

        /// <Summary>Initializes a new usb device list containing only devices matching a specific class GUID.</Summary>
        public LstK(KlstFlag flags, ref KLstPatternMatch patternMatch)
        {
            RuntimeHelpers.PrepareConstrainedRegions();

            try
            {
            }
            finally
            {
                var success = NativeMethods.LstK_InitEx(out handle, flags, ref patternMatch);
                var errorCode = Marshal.GetLastWin32Error();

                if (!success || handle.IsInvalid || handle.IsClosed)
                {
                    handle.SetHandleAsInvalid();
                    throw new Exception(GetType().Name + " failed. ErrorCode=" + errorCode.ToString("X"));
                }
            }
        }

        public KlstHandle Handle => handle;

        public virtual bool Free()
        {
            if (handle.IsInvalid || handle.IsClosed) return false;

            handle.Close();
            return true;
        }

        /// <Summary>Enumerates \ref KLST_DEVINFO elements of a \ref KLST_HANDLE.</Summary>
        public bool Enumerate(KLstEnumDevInfoCb enumDevListCb, IntPtr context)
        {
            return NativeMethods.LstK_Enumerate(handle, enumDevListCb, context);
        }

        /// <Summary>Gets the \ref KLST_DEVINFO element for the current position.</Summary>
        public bool Current(out KLstDevInfoHandle deviceInfo)
        {
            return NativeMethods.LstK_Current(handle, out deviceInfo);
        }

        /// <Summary>Advances the device list current \ref KLST_DEVINFO position.</Summary>
        public bool MoveNext(out KLstDevInfoHandle deviceInfo)
        {
            return NativeMethods.LstK_MoveNext(handle, out deviceInfo);
        }

        /// <Summary>Sets the device list to its initial position, which is before the first element in the list.</Summary>
        public void MoveReset()
        {
            NativeMethods.LstK_MoveReset(handle);
        }

        /// <Summary>Find a device by vendor and product id</Summary>
        public bool FindByVidPid(int vid, int pid, out KLstDevInfoHandle deviceInfo)
        {
            return NativeMethods.LstK_FindByVidPid(handle, vid, pid, out deviceInfo);
        }

        /// <Summary>Counts the number of device info elements in a device list.</Summary>
        public bool Count(ref uint count)
        {
            return NativeMethods.LstK_Count(handle, ref count);
        }
    }

    public class HotK
    {
        protected readonly KhotHandle handle;

        /// <Summary>Creates a new hot-plug handle for USB device arrival/removal event monitoring.</Summary>
        public HotK(ref KHotParams initParams)
        {
            RuntimeHelpers.PrepareConstrainedRegions();

            try
            {
            }
            finally
            {
                var success = NativeMethods.HotK_Init(out handle, ref initParams);
                var errorCode = Marshal.GetLastWin32Error();

                if (!success || handle.IsInvalid || handle.IsClosed)
                {
                    handle.SetHandleAsInvalid();
                    throw new Exception(GetType().Name + " failed. ErrorCode=" + errorCode.ToString("X"));
                }
            }
        }

        public KhotHandle Handle => handle;

        public virtual bool Free()
        {
            if (handle.IsInvalid || handle.IsClosed) return false;

            handle.Close();
            return true;
        }

        /// <Summary>Frees all hot-plug handles initialized with \ref HotK_Init.</Summary>
        public void FreeAll()
        {
            NativeMethods.HotK_FreeAll();
        }
    }

    public class UsbK
    {
        protected readonly KUsbDriverApi DriverApi;
        protected readonly KusbHandle handle;

        /// <Summary>Creates/opens a libusbK interface handle from the device list. This is a preferred method.</Summary>
        public UsbK(KLstDevInfoHandle devInfo)
        {
            if (!NativeMethods.LibK_LoadDriverAPI(out DriverApi, devInfo.DriverId))
            {
                var error = Marshal.GetLastWin32Error();

                throw new Exception(GetType().Name + " failed loading Driver API. ErrorCode=" + error.ToString("X"));
            }

            RuntimeHelpers.PrepareConstrainedRegions();

            bool success = DriverApi.Init(out handle, devInfo);

            if (!success || handle.IsInvalid || handle.IsClosed)
            {
                handle.SetHandleAsInvalid();

                throw new Exception(GetType().Name + " failed.");
            }
        }

        /// <Summary>Creates a libusbK handle for the device specified by a file handle.</Summary>
        public UsbK(IntPtr deviceHandle, KUsbDrvId driverId)
        {
            if (!NativeMethods.LibK_LoadDriverAPI(out DriverApi, (int) driverId))
            {
                var error = Marshal.GetLastWin32Error();

                throw new Exception(GetType().Name + " failed loading Driver API. ErrorCode=" + error.ToString("X"));
            }

            RuntimeHelpers.PrepareConstrainedRegions();

            bool success = DriverApi.Initialize(deviceHandle, out handle);

            if (!success || handle.IsInvalid || handle.IsClosed)
            {
                handle.SetHandleAsInvalid();

                throw new Exception(GetType().Name + " failed");
            }
        }

        public KusbHandle Handle => handle;

        public virtual bool Free()
        {
            if (handle.IsInvalid || handle.IsClosed) return false;

            handle.Close();
            return true;
        }

        /// <Summary>Claims the specified interface by number or index.</Summary>
        public bool ClaimInterface(byte numberOrIndex, bool isIndex)
        {
            return DriverApi.ClaimInterface(handle, numberOrIndex, isIndex);
        }

        /// <Summary>Releases the specified interface by number or index.</Summary>
        public bool ReleaseInterface(byte numberOrIndex, bool isIndex)
        {
            return DriverApi.ReleaseInterface(handle, numberOrIndex, isIndex);
        }

        /// <Summary>Sets the alternate setting of the specified interface.</Summary>
        public bool SetAltInterface(byte numberOrIndex, bool isIndex, byte altSettingNumber)
        {
            return DriverApi.SetAltInterface(handle, numberOrIndex, isIndex, altSettingNumber);
        }

        /// <Summary>Gets the alternate setting for the specified interface.</Summary>
        public bool GetAltInterface(byte numberOrIndex, bool isIndex, out byte altSettingNumber)
        {
            return DriverApi.GetAltInterface(handle, numberOrIndex, isIndex, out altSettingNumber);
        }

        /// <Summary>Gets the requested descriptor. This is a synchronous operation.</Summary>
        public bool GetDescriptor(byte descriptorType, byte index, ushort languageId, IntPtr buffer, uint bufferLength, out uint lengthTransferred)
        {
            return DriverApi.GetDescriptor(handle, descriptorType, index, languageId, buffer, bufferLength, out lengthTransferred);
        }

        /// <Summary>Gets the requested descriptor. This is a synchronous operation.</Summary>
        public bool GetDescriptor(byte descriptorType, byte index, ushort languageId, Array buffer, uint bufferLength, out uint lengthTransferred)
        {
            return DriverApi.GetDescriptor(handle,
                                           descriptorType,
                                           index,
                                           languageId,
                                           Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0),
                                           bufferLength,
                                           out lengthTransferred);
        }

        /// <Summary>Transmits control data over a default control endpoint.</Summary>
        public bool ControlTransfer(WinUsbSetupPacket setupPacket, IntPtr buffer, uint bufferLength, out uint lengthTransferred, IntPtr overlapped)
        {
            return DriverApi.ControlTransfer(handle, setupPacket, buffer, bufferLength, out lengthTransferred, overlapped);
        }

        /// <Summary>Transmits control data over a default control endpoint.</Summary>
        public bool ControlTransfer(WinUsbSetupPacket setupPacket, Array buffer, uint bufferLength, out uint lengthTransferred, IntPtr overlapped)
        {
            return DriverApi.ControlTransfer(handle, setupPacket, Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0), bufferLength, out lengthTransferred, overlapped);
        }

        /// <Summary>Transmits control data over a default control endpoint.</Summary>
        public bool ControlTransfer(WinUsbSetupPacket setupPacket, Array buffer, uint bufferLength, out uint lengthTransferred, KovlHandle overlapped)
        {
            return DriverApi.ControlTransfer(handle,
                                             setupPacket,
                                             Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0),
                                             bufferLength,
                                             out lengthTransferred,
                                             overlapped.DangerousGetHandle());
        }

        /// <Summary>Transmits control data over a default control endpoint.</Summary>
        public bool ControlTransfer(WinUsbSetupPacket setupPacket, IntPtr buffer, uint bufferLength, out uint lengthTransferred, KovlHandle overlapped)
        {
            return DriverApi.ControlTransfer(handle, setupPacket, buffer, bufferLength, out lengthTransferred, overlapped.DangerousGetHandle());
        }

        /// <Summary>Sets the power policy for a device.</Summary>
        public bool SetPowerPolicy(uint policyType, uint valueLength, IntPtr value)
        {
            return DriverApi.SetPowerPolicy(handle, policyType, valueLength, value);
        }

        /// <Summary>Sets the power policy for a device.</Summary>
        public bool SetPowerPolicy(uint policyType, uint valueLength, Array value)
        {
            return DriverApi.SetPowerPolicy(handle, policyType, valueLength, Marshal.UnsafeAddrOfPinnedArrayElement(value, 0));
        }

        /// <Summary>Gets the power policy for a device.</Summary>
        public bool GetPowerPolicy(uint policyType, ref uint valueLength, IntPtr Value)
        {
            return DriverApi.GetPowerPolicy(handle, policyType, ref valueLength, Value);
        }

        /// <Summary>Gets the power policy for a device.</Summary>
        public bool GetPowerPolicy(uint policyType, ref uint valueLength, Array value)
        {
            return DriverApi.GetPowerPolicy(handle, policyType, ref valueLength, Marshal.UnsafeAddrOfPinnedArrayElement(value, 0));
        }

        /// <Summary>Sets the device configuration number.</Summary>
        public bool SetConfiguration(byte configurationNumber)
        {
            return DriverApi.SetConfiguration(handle, configurationNumber);
        }

        /// <Summary>Gets the device current configuration number.</Summary>
        public bool GetConfiguration(out byte configurationNumber)
        {
            return DriverApi.GetConfiguration(handle, out configurationNumber);
        }

        /// <Summary>Resets the usb device of the specified interface handle. (port cycle).</Summary>
        public bool ResetDevice()
        {
            return NativeMethods.UsbK_ResetDevice(handle);
        }

        /// <Summary>Selects the specified interface by number or index as the current interface.</Summary>
        public bool SelectInterface(byte numberOrIndex, bool isIndex)
        {
            return DriverApi.SelectInterface(handle, numberOrIndex, isIndex);
        }

        /// <Summary>Retrieves a handle for an associated interface.</Summary>
        public bool GetAssociatedInterface(byte associatedInterfaceIndex, out KusbHandle associatedInterfaceHandle)
        {
            return DriverApi.GetAssociatedInterface(handle, associatedInterfaceIndex, out associatedInterfaceHandle);
        }

        /// <Summary>Clones the specified interface handle.</Summary>
        public bool Clone(out KusbHandle dstInterfaceHandle)
        {
            return DriverApi.Clone(handle, out dstInterfaceHandle);
        }

        /// <Summary>Retrieves the interface descriptor for the specified alternate interface settings for a particular interface handle.</Summary>
        public bool QueryInterfaceSettings(byte altSettingNumber, out UsbInterfaceDescriptor usbAltInterfaceDescriptor)
        {
            return DriverApi.QueryInterfaceSettings(handle, altSettingNumber, out usbAltInterfaceDescriptor);
        }

        /// <Summary>Retrieves information about the physical device that is associated with a libusbK handle.</Summary>
        public bool QueryDeviceInformation(uint informationType, ref uint bufferLength, IntPtr buffer)
        {
            return DriverApi.QueryDeviceInformation(handle, informationType, ref bufferLength, buffer);
        }

        /// <Summary>Sets the alternate setting of an interface.</Summary>
        public bool SetCurrentAlternateSetting(byte altSettingNumber)
        {
            return DriverApi.SetCurrentAlternateSetting(handle, altSettingNumber);
        }

        /// <Summary>Gets the current alternate interface setting for an interface.</Summary>
        public bool GetCurrentAlternateSetting(out byte altSettingNumber)
        {
            return DriverApi.GetCurrentAlternateSetting(handle, out altSettingNumber);
        }

        /// <Summary>Retrieves information about a pipe that is associated with an interface.</Summary>
        public bool QueryPipe(byte altSettingNumber, byte pipeIndex, out WinUsbPipeInformation pipeInformation)
        {
            return DriverApi.QueryPipe(handle, altSettingNumber, pipeIndex, out pipeInformation);
        }

        /// <Summary>Sets the policy for a specific pipe associated with an endpoint on the device. This is a synchronous operation.</Summary>
        public bool SetPipePolicy(byte pipeId, uint policyType, uint valueLength, IntPtr value)
        {
            return DriverApi.SetPipePolicy(handle, pipeId, policyType, valueLength, value);
        }

        /// <Summary>Sets the policy for a specific pipe associated with an endpoint on the device. This is a synchronous operation.</Summary>
        public bool SetPipePolicy(byte pipeId, uint policyType, uint valueLength, Array value)
        {
            return DriverApi.SetPipePolicy(handle, pipeId, policyType, valueLength, Marshal.UnsafeAddrOfPinnedArrayElement(value, 0));
        }

        /// <Summary>Gets the policy for a specific pipe (endpoint).</Summary>
        public bool GetPipePolicy(byte pipeId, uint policyType, ref uint valueLength, IntPtr value)
        {
            return DriverApi.GetPipePolicy(handle, pipeId, policyType, ref valueLength, value);
        }

        /// <Summary>Gets the policy for a specific pipe (endpoint).</Summary>
        public bool GetPipePolicy(byte pipeId, uint policyType, ref uint valueLength, Array value)
        {
            return DriverApi.GetPipePolicy(handle, pipeId, policyType, ref valueLength, Marshal.UnsafeAddrOfPinnedArrayElement(value, 0));
        }

        /// <Summary>Reads data from the specified pipe.</Summary>
        public bool ReadPipe(byte pipeId, IntPtr buffer, uint bufferLength, out uint lengthTransferred, IntPtr overlapped)
        {
            return DriverApi.ReadPipe(handle, pipeId, buffer, bufferLength, out lengthTransferred, overlapped);
        }

        /// <Summary>Reads data from the specified pipe.</Summary>
        public bool ReadPipe(byte pipeId, Array buffer, uint bufferLength, out uint lengthTransferred, IntPtr overlapped)
        {
            return DriverApi.ReadPipe(handle, pipeId, Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0), bufferLength, out lengthTransferred, overlapped);
        }

        /// <Summary>Reads data from the specified pipe.</Summary>
        public bool ReadPipe(byte pipeId, Array buffer, uint bufferLength, out uint lengthTransferred, KovlHandle overlapped)
        {
            return DriverApi.ReadPipe(handle,
                                      pipeId,
                                      Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0),
                                      bufferLength,
                                      out lengthTransferred,
                                      overlapped.DangerousGetHandle());
        }

        /// <Summary>Reads data from the specified pipe.</Summary>
        public bool ReadPipe(byte pipeId, IntPtr buffer, uint bufferLength, out uint lengthTransferred, KovlHandle overlapped)
        {
            return DriverApi.ReadPipe(handle, pipeId, buffer, bufferLength, out lengthTransferred, overlapped.DangerousGetHandle());
        }

        /// <Summary>Writes data to a pipe.</Summary>
        public bool WritePipe(byte pipeId, IntPtr buffer, uint bufferLength, out uint lengthTransferred, IntPtr overlapped)
        {
            return DriverApi.WritePipe(handle, pipeId, buffer, bufferLength, out lengthTransferred, overlapped);
        }

        /// <Summary>Writes data to a pipe.</Summary>
        public bool WritePipe(byte pipeId, Array buffer, uint bufferLength, out uint lengthTransferred, IntPtr overlapped)
        {
            return DriverApi.WritePipe(handle, pipeId, Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0), bufferLength, out lengthTransferred, overlapped);
        }

        /// <Summary>Writes data to a pipe.</Summary>
        public bool WritePipe(byte pipeId, Array buffer, uint bufferLength, out uint lengthTransferred, KovlHandle overlapped)
        {
            return DriverApi.WritePipe(handle,
                pipeId,
                                       Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0),
                                       bufferLength,
                                       out lengthTransferred,
                                       overlapped.DangerousGetHandle());
        }

        /// <Summary>Writes data to a pipe.</Summary>
        public bool WritePipe(byte pipeId, IntPtr buffer, uint bufferLength, out uint lengthTransferred, KovlHandle overlapped)
        {
            return DriverApi.WritePipe(handle, pipeId, buffer, bufferLength, out lengthTransferred, overlapped.DangerousGetHandle());
        }

        /// <Summary>Resets the data toggle and clears the stall condition on a pipe.</Summary>
        public bool ResetPipe(byte pipeId)
        {
            return DriverApi.ResetPipe(handle, pipeId);
        }

        /// <Summary>Aborts all of the pending transfers for a pipe.</Summary>
        public bool AbortPipe(byte pipeId)
        {
            return DriverApi.AbortPipe(handle, pipeId);
        }

        /// <Summary>Discards any data that is cached in a pipe.</Summary>
        public bool FlushPipe(byte pipeId)
        {
            return DriverApi.FlushPipe(handle, pipeId);
        }

        /// <Summary>Reads from an iso pipe.</Summary>
        public bool IsoReadPipe(byte pipeId, IntPtr buffer, uint bufferLength, IntPtr overlapped, KisoContext isoContext)
        {
            return DriverApi.IsoReadPipe(handle, pipeId, buffer, bufferLength, overlapped, isoContext);
        }

        /// <Summary>Reads from an iso pipe.</Summary>
        public bool IsoReadPipe(byte pipeId, Array buffer, uint bufferLength, IntPtr overlapped, KisoContext isoContext)
        {
            return DriverApi.IsoReadPipe(handle, pipeId, Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0), bufferLength, overlapped, isoContext);
        }

        /// <Summary>Reads from an iso pipe.</Summary>
        public bool IsoReadPipe(byte pipeId, Array buffer, uint bufferLength, KovlHandle overlapped, KisoContext isoContext)
        {
            return DriverApi.IsoReadPipe(handle, pipeId, Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0), bufferLength, overlapped.DangerousGetHandle(), isoContext);
        }

        /// <Summary>Reads from an iso pipe.</Summary>
        public bool IsoReadPipe(byte pipeId, IntPtr buffer, uint bufferLength, KovlHandle overlapped, KisoContext isoContext)
        {
            return DriverApi.IsoReadPipe(handle, pipeId, buffer, bufferLength, overlapped.DangerousGetHandle(), isoContext);
        }

        /// <Summary>Writes to an iso pipe.</Summary>
        public bool IsoWritePipe(byte pipeId, IntPtr buffer, uint bufferLength, IntPtr overlapped, KisoContext isoContext)
        {
            return DriverApi.IsoWritePipe(handle, pipeId, buffer, bufferLength, overlapped, isoContext);
        }

        /// <Summary>Writes to an iso pipe.</Summary>
        public bool IsoWritePipe(byte pipeId, Array buffer, uint bufferLength, IntPtr overlapped, KisoContext isoContext)
        {
            return DriverApi.IsoWritePipe(handle, pipeId, Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0), bufferLength, overlapped, isoContext);
        }

        /// <Summary>Writes to an iso pipe.</Summary>
        public bool IsoWritePipe(byte pipeId, Array buffer, uint bufferLength, KovlHandle overlapped, KisoContext isoContext)
        {
            return DriverApi.IsoWritePipe(handle, pipeId, Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0), bufferLength, overlapped.DangerousGetHandle(), isoContext);
        }

        /// <Summary>Writes to an iso pipe.</Summary>
        public bool IsoWritePipe(byte pipeId, IntPtr buffer, uint bufferLength, KovlHandle overlapped, KisoContext isoContext)
        {
            return DriverApi.IsoWritePipe(handle, pipeId, buffer, bufferLength, overlapped.DangerousGetHandle(), isoContext);
        }

        /// <Summary>Retrieves the current USB frame number.</Summary>
        public bool GetCurrentFrameNumber(out uint frameNumber)
        {
            return DriverApi.GetCurrentFrameNumber(handle, out frameNumber);
        }

        /// <Summary>Retrieves the results of an overlapped operation on the specified libusbK handle.</Summary>
        public bool GetOverlappedResult(IntPtr overlapped, out uint lpNumberOfBytesTransferred, bool bWait)
        {
            return DriverApi.GetOverlappedResult(handle, overlapped, out lpNumberOfBytesTransferred, bWait);
        }

        /// <Summary>Retrieves the results of an overlapped operation on the specified libusbK handle.</Summary>
        public bool GetOverlappedResult(KovlHandle overlapped, out uint lpNumberOfBytesTransferred, bool bWait)
        {
            return DriverApi.GetOverlappedResult(handle, overlapped.DangerousGetHandle(), out lpNumberOfBytesTransferred, bWait);
        }

        /// <Summary>Gets a USB device (driver specific) property from usb handle.</Summary>
        public bool GetProperty(KUsbProperty propertyType, ref uint propertySize, IntPtr value)
        {
            return DriverApi.GetProperty(handle, propertyType, ref propertySize, value);
        }

        /// <Summary>Gets a USB device (driver specific) property from usb handle.</Summary>
        public bool GetProperty(KUsbProperty propertyType, ref uint propertySize, Array value)
        {
            return DriverApi.GetProperty(handle, propertyType, ref propertySize, Marshal.UnsafeAddrOfPinnedArrayElement(value, 0));
        }
    }

    public class OvlK
    {
        protected readonly KovlPoolHandle handle;

        /// <Summary>Creates a new overlapped pool.</Summary>
        public OvlK(KusbHandle usbHandle, int maxOverlappedCount, KOvlPoolFlag flags)
        {
            RuntimeHelpers.PrepareConstrainedRegions();

            bool success = NativeMethods.OvlK_Init(out handle, usbHandle, maxOverlappedCount, flags);
            int errorCode = Marshal.GetLastWin32Error();

            if (!success || handle.IsInvalid || handle.IsClosed)
            {
                handle.SetHandleAsInvalid();
                throw new Exception(GetType().Name + " failed. ErrorCode=" + errorCode.ToString("X"));
            }
        }

        public KovlPoolHandle Handle => handle;

        public virtual bool Free()
        {
            if (handle.IsInvalid || handle.IsClosed) return false;

            handle.Close();
            return true;
        }

        /// <Summary>Gets a pre-allocated \c OverlappedK structure from the specified/default pool.</Summary>
        public bool Acquire(out KovlHandle overlappedK)
        {
            return NativeMethods.OvlK_Acquire(out overlappedK, handle);
        }

        /// <Summary>Returns an \c OverlappedK structure to it's pool.</Summary>
        public bool Release(KovlHandle overlappedK)
        {
            return NativeMethods.OvlK_Release(overlappedK);
        }

        /// <Summary>Returns the internal event handle used to signal IO operations.</Summary>
        public IntPtr GetEventHandle(KovlHandle overlappedK)
        {
            return NativeMethods.OvlK_GetEventHandle(overlappedK);
        }

        /// <Summary>Waits for overlapped I/O completion, and performs actions specified in \c WaitFlags.</Summary>
        public bool Wait(KovlHandle overlappedK, int timeoutMs, KOvlWaitFlag waitFlags, out uint transferredLength)
        {
            return NativeMethods.OvlK_Wait(overlappedK, timeoutMs, waitFlags, out transferredLength);
        }

        /// <Summary>Waits for overlapped I/O completion, cancels on a timeout error.</Summary>
        public bool WaitOrCancel(KovlHandle overlappedK, int timeoutMs, out uint transferredLength)
        {
            return NativeMethods.OvlK_WaitOrCancel(overlappedK, timeoutMs, out transferredLength);
        }

        /// <Summary>Waits for overlapped I/O completion, cancels on a timeout error and always releases the OvlK handle back to its pool.</Summary>
        public bool WaitAndRelease(KovlHandle overlappedK, int timeoutMs, out uint transferredLength)
        {
            return NativeMethods.OvlK_WaitAndRelease(overlappedK, timeoutMs, out transferredLength);
        }

        /// <Summary>Checks for i/o completion; returns immediately. (polling)</Summary>
        public bool IsComplete(KovlHandle overlappedK)
        {
            return NativeMethods.OvlK_IsComplete(overlappedK);
        }

        /// <Summary>Initializes an overlappedK for re-use. The overlappedK is not return to its pool.</Summary>
        public bool ReUse(KovlHandle overlappedK)
        {
            return NativeMethods.OvlK_ReUse(overlappedK);
        }
    }

    public class StmK
    {
        protected readonly KstmHandle handle;

        /// <Summary>Initializes a new uni-directional pipe stream.</Summary>
        public StmK(KusbHandle usbHandle, byte pipeId, int maxTransferSize, int maxPendingTransfers, int maxPendingIo, ref KStmCallback callbacks, KStmFlag flags)
        {
            RuntimeHelpers.PrepareConstrainedRegions();

            bool success = NativeMethods.StmK_Init(out handle, usbHandle, pipeId, maxTransferSize, maxPendingTransfers, maxPendingIo, ref callbacks, flags);
            int errorCode = Marshal.GetLastWin32Error();

            if (!success || handle.IsInvalid || handle.IsClosed)
            {
                handle.SetHandleAsInvalid();

                throw new Exception(GetType().Name + " failed. ErrorCode=" + errorCode.ToString("X"));
            }
        }

        public KstmHandle Handle => handle;

        public virtual bool Free()
        {
            if (handle.IsInvalid || handle.IsClosed) return false;

            handle.Close();
            return true;
        }

        /// <Summary>Starts the internal stream thread.</Summary>
        public bool Start()
        {
            return NativeMethods.StmK_Start(handle);
        }

        /// <Summary>Stops the internal stream thread.</Summary>
        public bool Stop(int timeoutCancelMs)
        {
            return NativeMethods.StmK_Stop(handle, timeoutCancelMs);
        }

        /// <Summary>Reads data from the stream buffer.</Summary>
        public bool Read(IntPtr buffer, int offset, int length, out uint transferredLength)
        {
            return NativeMethods.StmK_Read(handle, buffer, offset, length, out transferredLength);
        }

        /// <Summary>Reads data from the stream buffer.</Summary>
        public bool Read(Array buffer, int offset, int length, out uint transferredLength)
        {
            return NativeMethods.StmK_Read(handle, Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0), offset, length, out transferredLength);
        }

        /// <Summary>Writes data to the stream buffer.</Summary>
        public bool Write(IntPtr buffer, int offset, int length, out uint transferredLength)
        {
            return NativeMethods.StmK_Write(handle, buffer, offset, length, out transferredLength);
        }

        /// <Summary>Writes data to the stream buffer.</Summary>
        public bool Write(Array buffer, int offset, int length, out uint transferredLength)
        {
            return NativeMethods.StmK_Write(handle, Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0), offset, length, out transferredLength);
        }
    }

    public class IsoK
    {
        private static readonly int OfsFlags = Marshal.OffsetOf(typeof (KIsoContextMap), "Flags").ToInt32();
        private static readonly int OfsStartFrame = Marshal.OffsetOf(typeof (KIsoContextMap), "StartFrame").ToInt32();
        private static readonly int OfsErrorCount = Marshal.OffsetOf(typeof (KIsoContextMap), "ErrorCount").ToInt32();
        private static readonly int OfsNumberOfPackets = Marshal.OffsetOf(typeof (KIsoContextMap), "NumberOfPackets").ToInt32();

        protected readonly KisoContext handle;

        /// <Summary>Creates a new iso transfer context.</Summary>
        public IsoK(int numberOfPackets, int startFrame)
        {
            RuntimeHelpers.PrepareConstrainedRegions();

            bool success = NativeMethods.IsoK_Init(out handle, numberOfPackets, startFrame);
            int errorCode = Marshal.GetLastWin32Error();

            if (!success || handle.IsInvalid || handle.IsClosed)
            {
                handle.SetHandleAsInvalid();

                throw new Exception(GetType().Name + " failed. ErrorCode=" + errorCode.ToString("X"));
            }
        }

        public KisoContext Handle => handle;

        /// <Summary>Additional ISO transfer flags. See \ref KISO_FLAG.</Summary>
        public KisoFlag Flags
        {
            get => (KisoFlag) Marshal.ReadInt32(handle.DangerousGetHandle(), OfsFlags);
            set => Marshal.WriteInt32(handle.DangerousGetHandle(), OfsFlags, (int) value);
        }

        /// <Summary>Specifies the frame number that the transfer should begin on (0 for ASAP).</Summary>
        public uint StartFrame
        {
            get => (uint) Marshal.ReadInt32(handle.DangerousGetHandle(), OfsStartFrame);
            set => Marshal.WriteInt32(handle.DangerousGetHandle(), OfsStartFrame, (int) value);
        }

        /// <Summary>Contains the number of packets that completed with an error condition on return from the host controller driver.</Summary>
        public short ErrorCount
        {
            get => Marshal.ReadInt16(handle.DangerousGetHandle(), OfsErrorCount);
            set => Marshal.WriteInt16(handle.DangerousGetHandle(), OfsErrorCount, value);
        }

        /// <Summary>Specifies the number of packets that are described by the variable-length array member \c IsoPacket.</Summary>
        public short NumberOfPackets
        {
            get => Marshal.ReadInt16(handle.DangerousGetHandle(), OfsNumberOfPackets);
            set => Marshal.WriteInt16(handle.DangerousGetHandle(), OfsNumberOfPackets, value);
        }

        public virtual bool Free()
        {
            if (handle.IsInvalid || handle.IsClosed) return false;

            handle.Close();
            return true;
        }

        /// <Summary>Convenience function for setting the offset of all ISO packets of an iso transfer context.</Summary>
        public bool SetPackets(int packetSize)
        {
            return NativeMethods.IsoK_SetPackets(handle, packetSize);
        }

        /// <Summary>Convenience function for setting all fields of a \ref KISO_PACKET.</Summary>
        public bool SetPacket(int packetIndex, ref KIsoPacket isoPacket)
        {
            return NativeMethods.IsoK_SetPacket(handle, packetIndex, ref isoPacket);
        }

        /// <Summary>Convenience function for getting all fields of a \ref KISO_PACKET.</Summary>
        public bool GetPacket(int packetIndex, out KIsoPacket isoPacket)
        {
            return NativeMethods.IsoK_GetPacket(handle, packetIndex, out isoPacket);
        }

        /// <Summary>Convenience function for enumerating ISO packets of an iso transfer context.</Summary>
        public bool EnumPackets(KIsoEnumPacketsCb enumPackets, int startPacketIndex, IntPtr userState)
        {
            return NativeMethods.IsoK_EnumPackets(handle, enumPackets, startPacketIndex, userState);
        }

        /// <Summary>Convenience function for re-using an iso transfer context in a subsequent request.</Summary>
        public bool ReUse()
        {
            return NativeMethods.IsoK_ReUse(handle);
        }
    }
}
#pragma warning restore 649
#pragma warning restore 1591
