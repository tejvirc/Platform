namespace Aristocrat.Monaco.Hardware.VHD
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using Contracts.VHD;

    /// <summary>
    ///     VHD and ISO native methods: https://msdn.microsoft.com/en-us/library/windows/desktop/dd323700(v=vs.85).aspx
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    internal static class VirtualDiskNativeMethods
    {
        /// <summary>
        ///     Contains virtual hard disk (VHD) attach request parameters.
        /// </summary>
        [Flags]
        public enum AttachVirtualDiskFlag
        {
            /// <summary>
            ///     None
            /// </summary>
            AttachVirtualDiskFlagNone = 0x00000000,

            /// <summary>
            ///     Read Only
            /// </summary>
            AttachVirtualDiskFlagReadOnly = 0x00000001,

            /// <summary>
            ///     No Drive Letter
            /// </summary>
            AttachVirtualDiskFlagNoDriveLetter = 0x00000002,

            /// <summary>
            ///     Permanent Lifetime
            /// </summary>
            AttachVirtualDiskFlagPermanentLifetime = 0x00000004,

            /// <summary>
            ///     No Local Host
            /// </summary>
            AttachVirtualDiskFlagNoLocalHost = 0x00000008
        }

        /// <summary>
        ///     VHD version
        /// </summary>
        public enum AttachVirtualDiskVersion
        {
            /// <summary>
            ///     Unspecified
            /// </summary>
            AttachVirtualDiskVersionUnspecified = 0,

            /// <summary>
            ///     Version 1
            /// </summary>
            AttachVirtualDiskVersion1 = 1
        }

        /// <summary>
        ///     Com Class Context
        /// </summary>
        public enum ComClassContext : uint
        {
            /// <summary>
            ///     Inproc Server
            /// </summary>
            ClsctxInprocServer = 0x1,

            /// <summary>
            ///     Inproc Handler
            /// </summary>
            ClsctxInprocHandler = 0x2,

            /// <summary>
            ///     Local Server
            /// </summary>
            ClsctxLocalServer = 0x4,

            /// <summary>
            ///     Inproc Server 16
            /// </summary>
            ClsctxInprocServer16 = 0x8,

            /// <summary>
            ///     Remote Server
            /// </summary>
            ClsctxRemoteServer = 0x10,

            /// <summary>
            ///     Inproc Handler 16
            /// </summary>
            ClsctxInprocHandler16 = 0x20,

            /// <summary>
            ///     Reserved
            /// </summary>
            ClsctxReserved1 = 0x40,

            /// <summary>
            ///     Reserved
            /// </summary>
            ClsctxReserved2 = 0x80,

            /// <summary>
            ///     Reserved
            /// </summary>
            ClsctxReserved3 = 0x100,

            /// <summary>
            ///     Reserved
            /// </summary>
            ClsctxReserved4 = 0x200,

            /// <summary>
            ///     No Code Download
            /// </summary>
            ClsctxNoCodeDownload = 0x400,

            /// <summary>
            ///     Reserved
            /// </summary>
            ClsctxReserved5 = 2048,

            /// <summary>
            ///     No Custom Marshal
            /// </summary>
            ClsctxNoCustomMarshal = 0x1000,

            /// <summary>
            ///     Code Download
            /// </summary>
            ClsctxEnableCodeDownload = 0x2000,

            /// <summary>
            ///     No Failure Log
            /// </summary>
            ClsctxNoFailureLog = 0x4000,

            /// <summary>
            ///     Disable Aaa
            /// </summary>
            ClsctxDisableAaa = 0x8000,

            /// <summary>
            ///     Enable Aaa
            /// </summary>
            ClsctxEnableAaa = 0x10000,

            /// <summary>
            ///     Default Context
            /// </summary>
            ClsctxFromDefaultContext = 0x20000,

            /// <summary>
            ///     32 Bit Server
            /// </summary>
            ClsctxActivate32BitServer = 0x40000,

            /// <summary>
            ///     64 Bit Server
            /// </summary>
            ClsctxActivate64BitServer = 0x80000,

            /// <summary>
            ///     Enable Cloaking
            /// </summary>
            ClsctxEnableCloaking = 0x100000,

            /// <summary>
            ///     App Container
            /// </summary>
            ClsctxAppcontainer = 0x400000,

            /// <summary>
            ///     Activate Aaa As Iu
            /// </summary>
            ClsctxActivateAaaAsIu = 0x800000,

            /// <summary>
            ///     Ps Dll
            /// </summary>
            ClsctxPsDll = 0x80000000
        }

        /// <summary>
        ///     Create options
        /// </summary>
        public enum CreationDispositionFlags
        {
            /// <summary>
            ///     Create New
            /// </summary>
            CreateNew = 1,

            /// <summary>
            ///     Create Always
            /// </summary>
            CreateAlways = 2,

            /// <summary>
            ///     Open Existing
            /// </summary>
            OpenExisting = 3,

            /// <summary>
            ///     Open Always
            /// </summary>
            OpenAlways = 4,

            /// <summary>
            ///     Truncate Existing
            /// </summary>
            TruncateExisting = 5
        }

        /// <summary>
        ///     VHD Detach options
        /// </summary>
        public enum DetachVirtualDiskFlag
        {
            /// <summary>
            ///     None
            /// </summary>
            DetachVirtualDiskFlagNone = 0x00000000
        }

        /// <summary>
        ///     File share mode flags
        /// </summary>
        [Flags]
        public enum FileShareModeFlags
        {
            /// <summary>
            ///     Read Share
            /// </summary>
            FileShareRead = 0x00000001,

            /// <summary>
            ///     Write Share
            /// </summary>
            FileShareWrite = 0x00000002
        }

        /// <summary>
        ///     Generic access rights
        /// </summary>
        public enum GenericAccessRightsFlags : uint
        {
            /// <summary>
            ///     Read
            /// </summary>
            GenericRead = 0x80000000,

            /// <summary>
            ///     Write
            /// </summary>
            GenericWrite = 0x40000000,

            /// <summary>
            ///     Execute
            /// </summary>
            GenericExecute = 0x20000000,

            /// <summary>
            ///     All
            /// </summary>
            GenericAll = 0x10000000
        }

        /// <summary>
        ///     Contains virtual hard disk (VHD) information.
        /// </summary>
        public enum GetVirtualDiskInfoVersion
        {
            /// <summary>
            ///     Unspecficied
            /// </summary>
            GetVirtualDiskInfoUnspecified = 0,

            /// <summary>
            ///     Size
            /// </summary>
            GetVirtualDiskInfoSize = 1,

            /// <summary>
            ///     Identifier
            /// </summary>
            GetVirtualDiskInfoIdentifier = 2,

            /// <summary>
            ///     Parent Location
            /// </summary>
            GetVirtualDiskInfoParentLocation = 3,

            /// <summary>
            ///     Parent Identifier
            /// </summary>
            GetVirtualDiskInfoParentIdentifier = 4,

            /// <summary>
            ///     Timestamp
            /// </summary>
            GetVirtualDiskInfoParentTimestamp = 5,

            /// <summary>
            ///     Storage Type
            /// </summary>
            GetVirtualDiskInfoVirtualStorageType = 6,

            /// <summary>
            ///     Provider Subtype
            /// </summary>
            GetVirtualDiskInfoProviderSubtype = 7,

            /// <summary>
            ///     Is 4K Aligned
            /// </summary>
            GetVirtualDiskInfoIs4KAligned = 8,

            /// <summary>
            ///     Physical Disk
            /// </summary>
            GetVirtualDiskInfoPhysicalDisk = 9,

            /// <summary>
            ///     VHD Physical Sector Size
            /// </summary>
            GetVirtualDiskInfoVhdPhysicalSectorSize = 10, // 0xA

            /// <summary>
            ///     Smallest Safe Virtual Size
            /// </summary>
            GetVirtualDiskInfoSmallestSafeVirtualSize = 11,

            /// <summary>
            ///     Fragmentation
            /// </summary>
            GetVirtualDiskInfoFragmentation = 12
        }

        /// <summary>
        ///     IO Controls Code
        /// </summary>
        public enum IoControlCode : uint
        {
            /// <summary>
            ///     Get Volume Disk Extents
            /// </summary>
            GetVolumeDiskExtents = 5636096,

            /// <summary>
            ///     Storage Device Number
            /// </summary>
            StorageDeviceNumber = 2953344
        }

        /// <summary>
        ///     Open VHD options
        /// </summary>
        public enum OpenVirtualDiskFlag
        {
            /// <summary>
            ///     None
            /// </summary>
            OpenVirtualDiskFlagNone = 0x00000000,

            /// <summary>
            ///     No Parents
            /// </summary>
            OpenVirtualDiskFlagNoParents = 0x00000001,

            /// <summary>
            ///     Blank Drive
            /// </summary>
            OpenVirtualDiskFlagBlankFile = 0x00000002,

            /// <summary>
            ///     Boot Drive
            /// </summary>
            OpenVirtualDiskFlagBootDrive = 0x00000004
        }

        /// <summary>
        ///     Open VHD verions
        /// </summary>
        public enum OpenVirtualDiskVersion
        {
            /// <summary>
            ///     Version 1
            /// </summary>
            OpenVirtualDiskVersion1 = 1
        }

        /// <summary>
        ///     VHD access options
        /// </summary>
        [Flags]
        public enum VirtualDiskAccessMask
        {
            /// <summary>
            ///     Attach Read Only
            /// </summary>
            VirtualDiskAccessAttachRo = 0x00010000,

            /// <summary>
            ///     Attach Read Write
            /// </summary>
            VirtualDiskAccessAttachRw = 0x00020000,

            /// <summary>
            ///     Detach
            /// </summary>
            VirtualDiskAccessDetach = 0x00040000,

            /// <summary>
            ///     Get Info
            /// </summary>
            VirtualDiskAccessGetInfo = 0x00080000,

            /// <summary>
            ///     Create
            /// </summary>
            VirtualDiskAccessCreate = 0x00100000,

            /// <summary>
            ///     Metaops
            /// </summary>
            VirtualDiskAccessMetaops = 0x00200000,

            /// <summary>
            ///     Read
            /// </summary>
            VirtualDiskAccessRead = 0x000d0000,

            /// <summary>
            ///     All
            /// </summary>
            VirtualDiskAccessAll = 0x003f0000,

            /// <summary>
            ///     Writable
            /// </summary>
            VirtualDiskAccessWritable = 0x00320000
        }

        /// <summary>
        ///     Indicates success
        /// </summary>
        public const int Success = 0;

        /// <summary>
        ///     VHD open depth
        /// </summary>
        public const int OpenVirtualDiskRwDepthDefault = 1;

        /// <summary>
        ///     ISO Type
        /// </summary>
        public const int VirtualStorageTypeDeviceIso = 1;

        /// <summary>
        ///     VHD Type
        /// </summary>
        public const int VirtualStorageTypeDeviceVhd = 2;

        /// <summary>
        ///     Indicates an invalid handle
        /// </summary>
        public static readonly IntPtr InvalidHandleValue = (IntPtr)(-1);

        /// <summary>
        ///     Microsoft
        /// </summary>
        public static readonly Guid VirtualStorageTypeVendorMicrosoft =
            new Guid("EC984AEC-A0F9-47e9-901F-71415A66345B");

        /// <summary>
        ///     Unknown vendor
        /// </summary>
        public static readonly Guid VirtualStorageTypeVendorUnknown = Guid.Empty;

        /// <summary>
        ///     Attaches a virtual hard disk (VHD) or CD or DVD image file (ISO) by locating an appropriate VHD provider to
        ///     accomplish the attachment.
        /// </summary>
        /// <param name="virtualDiskHandle">A handle to an open virtual disk</param>
        /// <param name="securityDescriptor">An optional pointer to a SECURITY_DESCRIPTOR to apply to the attached virtual disk</param>
        /// <param name="flags">A valid combination of values of the ATTACH_VIRTUAL_DISK_FLAG enumeration</param>
        /// <param name="providerSpecificFlags">
        ///     Flags specific to the type of virtual disk being attached. May be zero if none are
        ///     required
        /// </param>
        /// <param name="parameters">
        ///     A pointer to a valid ATTACH_VIRTUAL_DISK_PARAMETERS structure that contains attachment
        ///     parameter data.
        /// </param>
        /// <param name="overlapped">An optional pointer to a valid OVERLAPPED structure if asynchronous operation is desired.</param>
        /// <returns>Status of the request.</returns>
        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern int AttachVirtualDisk(
            VirtualDiskHandle virtualDiskHandle,
            IntPtr securityDescriptor,
            AttachVirtualDiskFlag flags,
            int providerSpecificFlags,
            ref AttachVirtualDiskParameters parameters,
            IntPtr overlapped);

        /// <summary>
        ///     Detaches a virtual hard disk (VHD) or CD or DVD image file (ISO) by locating an appropriate virtual disk provider
        ///     to
        ///     accomplish the operation.
        /// </summary>
        /// <param name="virtualDiskHandle">
        ///     A handle to an open virtual disk, which must have been opened using the
        ///     VIRTUAL_DISK_ACCESS_DETACH flag set in the VirtualDiskAccessMask parameter to the OpenVirtualDisk function. For
        ///     information on how to open a virtual disk, see the OpenVirtualDisk function.
        /// </param>
        /// <param name="flags">A valid combination of values of the DETACH_VIRTUAL_DISK_FLAG enumeration.</param>
        /// <param name="providerSpecificFlags">
        ///     Flags specific to the type of virtual disk being detached. May be zero if none are
        ///     required.
        /// </param>
        /// <returns>Status of the request.</returns>
        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern int DetachVirtualDisk(
            VirtualDiskHandle virtualDiskHandle,
            DetachVirtualDiskFlag flags,
            int providerSpecificFlags);

        /// <summary>
        ///     Closes the handle
        /// </summary>
        /// <param name="virtualDiskHandle">The handle to close</param>
        /// <returns>true if successful</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern bool CloseHandle(IntPtr virtualDiskHandle);

        /// <summary>
        ///     Opens a virtual hard disk (VHD) or CD or DVD image file (ISO) for use.
        /// </summary>
        /// <param name="virtualStorageType">A pointer to a valid VIRTUAL_STORAGE_TYPE structure.</param>
        /// <param name="path">A pointer to a valid path to the virtual disk image to open.</param>
        /// <param name="virtualDiskAccessMask">A valid value of the VIRTUAL_DISK_ACCESS_MASK enumeration.</param>
        /// <param name="flags">A valid combination of values of the OPEN_VIRTUAL_DISK_FLAG enumeration.</param>
        /// <param name="parameters">An optional pointer to a valid OPEN_VIRTUAL_DISK_PARAMETERS structure. Can be NULL.</param>
        /// <param name="handle">A pointer to the handle object that represents the open virtual disk.</param>
        /// <returns>
        ///     If the function succeeds, the return value is ERROR_SUCCESS (0) and the Handle parameter contains a valid
        ///     pointer to the new virtual disk object.
        /// </returns>
        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern int OpenVirtualDisk(
            ref VirtualStorageType virtualStorageType,
            string path,
            VirtualDiskAccessMask virtualDiskAccessMask,
            OpenVirtualDiskFlag flags,
            ref OpenVirtualDiskParameters parameters,
            ref VirtualDiskHandle handle);

        /// <summary>
        ///     Retrieves the path to the physical device object that contains a virtual hard disk (VHD) or CD or DVD image file
        ///     (ISO).
        /// </summary>
        /// <param name="virtualDiskHandle">
        ///     A handle to the open virtual disk, which must have been opened using the
        ///     VIRTUAL_DISK_ACCESS_GET_INFO flag.
        /// </param>
        /// <param name="diskPathSizeInBytes">The size, in bytes, of the buffer pointed to by the DiskPath parameter.</param>
        /// <param name="diskPath">A target buffer to receive the path of the physical disk device that contains the virtual disk.</param>
        /// <returns>
        ///     If the function succeeds, the return value is ERROR_SUCCESS and the DiskPath parameter contains a pointer to a
        ///     populated string.
        /// </returns>
        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern int GetVirtualDiskPhysicalPath(
            VirtualDiskHandle virtualDiskHandle,
            ref int diskPathSizeInBytes,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder diskPath);

        /// <summary>
        ///     Retrieves information about a virtual hard disk (VHD).
        /// </summary>
        /// <param name="virtualDiskHandle">
        ///     A handle to the open VHD, which must have been opened using the
        ///     VIRTUAL_DISK_ACCESS_GET_INFO flag set in the VirtualDiskAccessMask parameter to the OpenVirtualDisk function.
        /// </param>
        /// <param name="virtualDiskInfoSize">A pointer to a ULONG that contains the size of the VirtualDiskInfo parameter.</param>
        /// <param name="virtualDiskInfo">
        ///     A pointer to a valid GET_VIRTUAL_DISK_INFO structure. The format of the data returned is
        ///     dependent on the value passed in the Version member by the caller.
        /// </param>
        /// <param name="sizeUsed">A pointer to a ULONG that contains the size used.</param>
        /// <returns>
        ///     If the function succeeds, the return value is ERROR_SUCCESS and the VirtualDiskInfo parameter contains the
        ///     requested information.
        /// </returns>
        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern int GetVirtualDiskInformation(
            VirtualDiskHandle virtualDiskHandle,
            ref uint virtualDiskInfoSize,
            ref GetVirtualDiskInfoSize virtualDiskInfo,
            IntPtr sizeUsed);

        /// <summary>
        ///     Finds the first volume.
        /// </summary>
        /// <param name="volumeName">The volume name</param>
        /// <param name="bufferLength">The length of the buffer.</param>
        /// <returns>The volume handle</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern IntPtr FindFirstVolume(StringBuilder volumeName, int bufferLength);

        /// <summary>
        ///     Closes the volume handle
        /// </summary>
        /// <param name="findVolumeHandle">The volume handle</param>
        /// <returns>true if closed</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FindVolumeClose(IntPtr findVolumeHandle);

        /// <summary>
        ///     Finds the next volume
        /// </summary>
        /// <param name="findVolumeHandle">The volume handle</param>
        /// <param name="volumeName">The volume name</param>
        /// <param name="bufferLength">The length of the buffer</param>
        /// <returns>true if successful</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FindNextVolume(IntPtr findVolumeHandle, StringBuilder volumeName, int bufferLength);

        /// <summary>
        ///     Creates a file
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <param name="desiredAccess">The desired file access</param>
        /// <param name="shareMode">The share mode</param>
        /// <param name="securityAttribute">The security attributes</param>
        /// <param name="creationDisposition">The creation disposition</param>
        /// <param name="flagsAndAttributes">The file flags and attributes</param>
        /// <param name="templateFile">The template file</param>
        /// <returns>The file handle if successful</returns>
        [DllImport(
            "kernel32.dll",
            CharSet = CharSet.Unicode,
            SetLastError = true,
            BestFitMapping = false,
            ThrowOnUnmappableChar = true)]
        public static extern IntPtr CreateFile(
            [MarshalAs(UnmanagedType.LPTStr)] string fileName,
            GenericAccessRightsFlags desiredAccess,
            FileShareModeFlags shareMode,
            IntPtr securityAttribute,
            CreationDispositionFlags creationDisposition,
            int flagsAndAttributes,
            IntPtr templateFile);

        /// <summary>
        ///     Sends a control code directly to a specified device driver, causing the corresponding device to perform the
        ///     corresponding operation.
        /// </summary>
        /// <param name="deviceHandle">
        ///     A handle to the device on which the operation is to be performed. The device is typically a
        ///     volume, directory, file, or stream. To retrieve a device handle, use the CreateFile function.
        /// </param>
        /// <param name="controlCode">
        ///     The control code for the operation. This value identifies the specific operation to be
        ///     performed and the type of device on which to perform it.
        /// </param>
        /// <param name="inBuffer">
        ///     A pointer to the input buffer that contains the data required to perform the operation. The
        ///     format of this data depends on the value of the dwIoControlCode parameter.
        /// </param>
        /// <param name="inBufferSize">The size of the input buffer, in bytes.</param>
        /// <param name="outBuffer">
        ///     A pointer to the output buffer that is to receive the data returned by the operation. The
        ///     format of this data depends on the value of the dwIoControlCode parameter.
        /// </param>
        /// <param name="outBufferSize">The size of the output buffer, in bytes.</param>
        /// <param name="bytesReturned">
        ///     A pointer to a variable that receives the size of the data stored in the output buffer, in
        ///     bytes.
        /// </param>
        /// <param name="overlapped">A pointer to an OVERLAPPED structure.</param>
        /// <returns>If the operation completes successfully, the return value is nonzero.</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeviceIoControl(
            IntPtr deviceHandle,
            IoControlCode controlCode,
            IntPtr inBuffer,
            uint inBufferSize,
            ref StorageDeviceNumber outBuffer,
            uint outBufferSize,
            ref uint bytesReturned,
            IntPtr overlapped);

        /// <summary>
        ///     Associates a volume with a drive letter or a directory on another volume.
        /// </summary>
        /// <param name="mountPoint">
        ///     The user-mode path to be associated with the volume. This may be a drive letter (for example,
        ///     "X:\") or a directory on another volume (for example, "Y:\MountX\"). The string must end with a trailing backslash
        ///     ('\').
        /// </param>
        /// <param name="volumeName">
        ///     A volume GUID path for the volume. This string must be of the form "\\?\Volume{GUID}\" where
        ///     GUID is a GUID that identifies the volume. The "\\?\" turns off path parsing and is ignored as part of the path,
        /// </param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetVolumeMountPoint(string mountPoint, string volumeName);

        /// <summary>
        ///     Deletes the volume mount point
        /// </summary>
        /// <param name="volumeName">The volume name</param>
        /// <returns>true upon success, otherwise false</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteVolumeMountPoint(string volumeName);

        /// <summary>
        ///     The disk extent
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DiskExtent
        {
            /// <summary>
            ///     The disk number
            /// </summary>
            public int diskNumber;

            /// <summary>
            ///     The starting offset
            /// </summary>
            public long startingOffset;

            /// <summary>
            ///     The extent length
            /// </summary>
            public long extentLength;
        }

        /// <summary>
        ///     Volume disk extents
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VolumeDiskExtents
        {
            /// <summary>
            ///     The number of disk extents
            /// </summary>
            public int numberOfDiskExtents;

            /// <summary>
            ///     Disk extents
            /// </summary>
            public DiskExtent[] extents;
        }

        /// <summary>
        ///     AttachVirtualDiskParameters
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct AttachVirtualDiskParameters
        {
            /// <summary>
            ///     AttachVirtualDiskVersion
            /// </summary>
            public AttachVirtualDiskVersion version;

            /// <summary>
            ///     AttachVirtualDiskParametersVersion1
            /// </summary>
            public AttachVirtualDiskParametersVersion1 version1;
        }

        /// <summary>
        ///     AttachVirtualDiskParametersVersion1
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct AttachVirtualDiskParametersVersion1
        {
            /// <summary>
            ///     reserved
            /// </summary>
            public int reserved;
        }

        /// <summary>
        ///     OpenVirtualDiskParameters
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct OpenVirtualDiskParameters
        {
            /// <summary>
            ///     OpenVirtualDiskVersion
            /// </summary>
            public OpenVirtualDiskVersion version;

            /// <summary>
            ///     OpenVirtualDiskParametersVersion1
            /// </summary>
            public OpenVirtualDiskParametersVersion1 version1;
        }

        /// <summary>
        ///     OpenVirtualDiskParametersVersion1
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct OpenVirtualDiskParametersVersion1
        {
            /// <summary>
            ///     rwDepth
            /// </summary>
            public int rwDepth;
        }

        /// <summary>
        ///     VirtualStorageType
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct VirtualStorageType
        {
            /// <summary>
            ///     deviceId
            /// </summary>
            public int deviceId;

            /// <summary>
            ///     vendorId
            /// </summary>
            public Guid vendorId;
        }

        /// <summary>
        ///     StorageDeviceNumber
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct StorageDeviceNumber
        {
            /// <summary>
            ///     deviceType
            /// </summary>
            public int deviceType;

            /// <summary>
            ///     deviceNumber
            /// </summary>
            public int deviceNumber;

            /// <summary>
            ///     partitionNumber
            /// </summary>
            public int partitionNumber;
        }

        /// <summary>
        ///     GetVirtualDiskInfoSize
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct GetVirtualDiskInfoSize
        {
            /// <summary>
            ///     GetVirtualDiskInfoVersion
            /// </summary>
            public GetVirtualDiskInfoVersion version;

            /// <summary>
            ///     virtualSize
            /// </summary>
            public ulong virtualSize;

            /// <summary>
            ///     physicalSize
            /// </summary>
            public ulong physicalSize;

            /// <summary>
            ///     blockSize
            /// </summary>
            public uint blockSize;

            /// <summary>
            ///     sectorSize
            /// </summary>
            public uint sectorSize;
        }
    }
}