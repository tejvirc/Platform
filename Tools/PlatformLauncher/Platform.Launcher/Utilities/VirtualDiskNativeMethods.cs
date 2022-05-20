namespace Platform.Launcher.Utilities
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [SuppressUnmanagedCodeSecurity]
    internal static class VirtualDiskNativeMethods
    {
        public const int Success = 0;
        public const int OpenVirtualDiskRwDepthDefault = 1;
        public const int VirtualStorageTypeDeviceIso = 1;
        public const int VirtualStorageTypeDeviceVhd = 2;

        public static readonly IntPtr InvalidHandleValue = (IntPtr)(-1);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Not currently used, but required for VHDs")]
        public static readonly Guid VirtualStorageTypeVendorMicrosoft =
            new Guid("EC984AEC-A0F9-47e9-901F-71415A66345B");

        public static readonly Guid VirtualStorageTypeVendorUnknown = Guid.Empty;

        [Flags]
        public enum AttachVirtualDiskFlag
        {
            AttachVirtualDiskFlagNone = 0x00000000,
            AttachVirtualDiskFlagReadOnly = 0x00000001,
            AttachVirtualDiskFlagNoDriveLetter = 0x00000002,
            AttachVirtualDiskFlagPermanentLifetime = 0x00000004,
            AttachVirtualDiskFlagNoLocalHost = 0x00000008
        }

        public enum AttachVirtualDiskVersion
        {
            AttachVirtualDiskVersionUnspecified = 0,
            AttachVirtualDiskVersion1 = 1
        }

        public enum ComClassContext : uint
        {
            ClsctxInprocServer = 0x1,
            ClsctxInprocHandler = 0x2,
            ClsctxLocalServer = 0x4,
            ClsctxInprocServer16 = 0x8,
            ClsctxRemoteServer = 0x10,
            ClsctxInprocHandler16 = 0x20,
            ClsctxReserved1 = 0x40,
            ClsctxReserved2 = 0x80,
            ClsctxReserved3 = 0x100,
            ClsctxReserved4 = 0x200,
            ClsctxNoCodeDownload = 0x400,
            ClsctxReserved5 = 2048,
            ClsctxNoCustomMarshal = 0x1000,
            ClsctxEnableCodeDownload = 0x2000,
            ClsctxNoFailureLog = 0x4000,
            ClsctxDisableAaa = 0x8000,
            ClsctxEnableAaa = 0x10000,
            ClsctxFromDefaultContext = 0x20000,
            ClsctxActivate32BitServer = 0x40000,
            ClsctxActivate64BitServer = 0x80000,
            ClsctxEnableCloaking = 0x100000,
            ClsctxAppcontainer = 0x400000,
            ClsctxActivateAaaAsIu = 0x800000,
            ClsctxPsDll = 0x80000000
        }

        public enum CreationDispositionFlags
        {
            CreateNew = 1,
            CreateAlways = 2,
            OpenExisting = 3,
            OpenAlways = 4,
            TruncateExisting = 5
        }

        public enum DetachVirtualDiskFlag
        {
            DetachVirtualDiskFlagNone = 0x00000000
        }

        [Flags]
        public enum FileShareModeFlags
        {
            FileShareRead = 0x00000001,
            FileShareWrite = 0x00000002
        }

        public enum GenericAccessRightsFlags : uint
        {
            GenericRead = 0x80000000,
            GenericWrite = 0x40000000,
            GenericExecute = 0x20000000,
            GenericAll = 0x10000000
        }

        public enum GetVirtualDiskInfoVersion
        {
            GetVirtualDiskInfoUnspecified = 0,
            GetVirtualDiskInfoSize = 1,
            GetVirtualDiskInfoIdentifier = 2,
            GetVirtualDiskInfoParentLocation = 3,
            GetVirtualDiskInfoParentIdentifier = 4,
            GetVirtualDiskInfoParentTimestamp = 5,
            GetVirtualDiskInfoVirtualStorageType = 6,
            GetVirtualDiskInfoProviderSubtype = 7,
            GetVirtualDiskInfoIs_4KAligned = 8,
            GetVirtualDiskInfoPhysicalDisk = 9,
            GetVirtualDiskInfoVhdPhysicalSectorSize = 10, // 0xA
            GetVirtualDiskInfoSmallestSafeVirtualSize = 11,
            GetVirtualDiskInfoFragmentation = 12
        }

        public enum IoControlCode : uint
        {
            GetVolumeDiskExtents = 5636096,
            StorageDeviceNumber = 2953344
        }

        public enum OpenVirtualDiskFlag
        {
            OpenVirtualDiskFlagNone = 0x00000000,
            OpenVirtualDiskFlagNoParents = 0x00000001,
            OpenVirtualDiskFlagBlankFile = 0x00000002,
            OpenVirtualDiskFlagBootDrive = 0x00000004
        }

        public enum OpenVirtualDiskVersion
        {
            OpenVirtualDiskVersion1 = 1
        }

        [Flags]
        public enum VirtualDiskAccessMask
        {
            VirtualDiskAccessAttachRo = 0x00010000,
            VirtualDiskAccessAttachRw = 0x00020000,
            VirtualDiskAccessDetach = 0x00040000,
            VirtualDiskAccessGetInfo = 0x00080000,
            VirtualDiskAccessCreate = 0x00100000,
            VirtualDiskAccessMetaops = 0x00200000,
            VirtualDiskAccessRead = 0x000d0000,
            VirtualDiskAccessAll = 0x003f0000,
            VirtualDiskAccessWritable = 0x00320000
        }

        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int AttachVirtualDisk(
            VirtualDiskHandle virtualDiskHandle,
            IntPtr securityDescriptor,
            AttachVirtualDiskFlag flags,
            int providerSpecificFlags,
            ref AttachVirtualDiskParameters parameters,
            IntPtr overlapped);

        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int DetachVirtualDisk(
            VirtualDiskHandle virtualDiskHandle,
            DetachVirtualDiskFlag flags,
            int providerSpecificFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern bool CloseHandle(IntPtr virtualDiskHandle);

        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int OpenVirtualDisk(
            ref VirtualStorageType virtualStorageType,
            string path,
            VirtualDiskAccessMask virtualDiskAccessMask,
            OpenVirtualDiskFlag flags,
            ref OpenVirtualDiskParameters parameters,
            ref VirtualDiskHandle handle);

        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern int GetVirtualDiskPhysicalPath(
            VirtualDiskHandle virtualDiskHandle,
            ref int diskPathSizeInBytes,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder diskPath);

        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern int GetVirtualDiskInformation(
            VirtualDiskHandle virtualDiskHandle,
            ref uint virtualDiskInfoSize,
            ref GetVirtualDiskInfoSize virtualDiskInfo,
            IntPtr sizeUsed);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern IntPtr FindFirstVolume(StringBuilder volumeName, int bufferLength);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FindVolumeClose(IntPtr findVolumeHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FindNextVolume(IntPtr findVolumeHandle, StringBuilder volumeName, int bufferLength);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern IntPtr CreateFile(
            [MarshalAs(UnmanagedType.LPTStr)] string fileName,
            GenericAccessRightsFlags desiredAccess,
            FileShareModeFlags shareMode,
            IntPtr securityAttribute,
            CreationDispositionFlags creationDisposition,
            int flagsAndAttributes,
            IntPtr templateFile);

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

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetVolumeMountPoint(string mountPoint, string volumeName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteVolumeMountPoint(string volumeName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool GetVolumeNameForVolumeMountPoint(string mountPoint, [Out] StringBuilder volumeName, uint length);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DiskExtent
        {
            public int diskNumber;

            public long startingOffset;

            public long extentLength;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VolumeDiskExtents
        {
            public int numberOfDiskExtents;

            public DiskExtent[] extents;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct AttachVirtualDiskParameters
        {
            public AttachVirtualDiskVersion version;
            public AttachVirtualDiskParametersVersion1 version1;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct AttachVirtualDiskParametersVersion1
        {
            public int reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct OpenVirtualDiskParameters
        {
            public OpenVirtualDiskVersion version;
            public OpenVirtualDiskParametersVersion1 version1;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct OpenVirtualDiskParametersVersion1
        {
            public int rwDepth;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct VirtualStorageType
        {
            public int deviceId;

            public Guid vendorId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct StorageDeviceNumber
        {
            public int deviceType;
            public int deviceNumber;
            public int partitionNumber;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct GetVirtualDiskInfoSize
        {
            public GetVirtualDiskInfoVersion version;
            public ulong virtualSize;
            public ulong physicalSize;
            public uint blockSize;
            public uint sectorSize;
        }
    }
}