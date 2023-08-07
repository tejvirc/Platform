namespace Aristocrat.Monaco.Hardware.VHD
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Contracts.VHD;
    using Kernel;
    using log4net;

    /// <summary>
    ///     An <see cref="IVirtualDisk" /> implementation
    /// </summary>
    public class VirtualDisk : IVirtualDisk
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventBus _bus;

        public VirtualDisk()
            : this(ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        public VirtualDisk(IEventBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IVirtualDisk) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public VirtualDiskHandle AttachImage(string file, string path)
        {
            Logger.Debug($"Preparing to attach image {file} at mount point {path}");

            var attachParameters = new VirtualDiskNativeMethods.AttachVirtualDiskParameters
            {
                version = VirtualDiskNativeMethods.AttachVirtualDiskVersion.AttachVirtualDiskVersion1
            };

            var before = GetVolumes();

            var handle = Open(
                file,
                VirtualDiskNativeMethods.VirtualDiskAccessMask.VirtualDiskAccessAttachRo |
                VirtualDiskNativeMethods.VirtualDiskAccessMask.VirtualDiskAccessGetInfo);
            if (handle.IsInvalid)
            {
                return handle;
            }

            const VirtualDiskNativeMethods.AttachVirtualDiskFlag flags =
                VirtualDiskNativeMethods.AttachVirtualDiskFlag.AttachVirtualDiskFlagReadOnly |
                VirtualDiskNativeMethods.AttachVirtualDiskFlag.AttachVirtualDiskFlagNoDriveLetter;

            var result = VirtualDiskNativeMethods.AttachVirtualDisk(
                handle,
                IntPtr.Zero,
                flags,
                0,
                ref attachParameters,
                IntPtr.Zero);
            if (result != VirtualDiskNativeMethods.Success)
            {
                Logger.Warn($"Failed to attach virtual disk {file}");

                handle.Close();
                handle.SetHandleAsInvalid();

                return handle;
            }

            var after = GetVolumes();

            // TODO: FindVolumePath only works for VHD files
            var volumePath = after.FirstOrDefault(v => before.All(v2 => v != v2));

            if (!MountToDriveLetter(volumePath, path))
            {
                Logger.Warn($"Failed to mount {path} to volume {volumePath}");

                handle.Close();
                handle.SetHandleAsInvalid();

                return handle;
            }

            _bus.Publish(new DiskMountedEvent(volumePath, file, path));

            return handle;
        }

        /// <inheritdoc />
        public bool DetachImage(VirtualDiskHandle handle, string path)
        {
            if (!handle.IsClosed)
            {
                if (VirtualDiskNativeMethods.DetachVirtualDisk(
                        handle,
                        VirtualDiskNativeMethods.DetachVirtualDiskFlag.DetachVirtualDiskFlagNone,
                        0) != VirtualDiskNativeMethods.Success)
                {
                    Logger.Warn("Failed to detach virtual disk");
                }
            }

            _bus.Publish(new DiskUnmountedEvent(path));

            return VirtualDiskNativeMethods.DeleteVolumeMountPoint(path);
        }

        /// <inheritdoc />
        public bool DetachImage(string file, string path)
        {
            var handle = Open(file, VirtualDiskNativeMethods.VirtualDiskAccessMask.VirtualDiskAccessDetach);
            if (handle == null)
            {
                return false;
            }

            try
            {
                return DetachImage(handle, path);
            }
            finally
            {
                handle.Close();
            }
        }

        /// <inheritdoc />
        public void Close(VirtualDiskHandle handle)
        {
            if (!handle.IsInvalid && !handle.IsClosed)
            {
                handle.Close();
            }
        }

        private static VirtualDiskHandle Open(
            string file,
            VirtualDiskNativeMethods.VirtualDiskAccessMask accessMask)
        {
            var openParameters = new VirtualDiskNativeMethods.OpenVirtualDiskParameters
            {
                version = VirtualDiskNativeMethods.OpenVirtualDiskVersion.OpenVirtualDiskVersion1,
                version1 =
                {
                    rwDepth = VirtualDiskNativeMethods.OpenVirtualDiskRwDepthDefault
                }
            };

            var openStorageType = new VirtualDiskNativeMethods.VirtualStorageType
            {
                deviceId = VirtualDiskNativeMethods.VirtualStorageTypeDeviceIso,
                vendorId = VirtualDiskNativeMethods.VirtualStorageTypeVendorUnknown
            };

            var handle = new VirtualDiskHandle();

            var result = VirtualDiskNativeMethods.OpenVirtualDisk(
                ref openStorageType,
                file,
                accessMask,
                VirtualDiskNativeMethods.OpenVirtualDiskFlag.OpenVirtualDiskFlagNone,
                ref openParameters,
                ref handle);
            if (result != VirtualDiskNativeMethods.Success)
            {
                Logger.Warn($"Failed to open virtual disk {file}");
            }

            return handle;
        }

        private static bool MountToDriveLetter(string vhdVolumePath, string mountPoint)
        {
            if (vhdVolumePath[vhdVolumePath.Length - 1] != '\\')
            {
                vhdVolumePath += '\\';
            }

            if (mountPoint[mountPoint.Length - 1] != '\\')
            {
                mountPoint += '\\';
            }

            return VirtualDiskNativeMethods.SetVolumeMountPoint(mountPoint, vhdVolumePath);
        }

        private static IEnumerable<string> GetVolumes()
        {
            var volumes = new List<string>();
            var volumeName = new StringBuilder(260);

            var findVolumeHandle = VirtualDiskNativeMethods.FindFirstVolume(volumeName, volumeName.Capacity);
            do
            {
                var backslashPos = volumeName.Length - 1;
                if (volumeName[backslashPos] == '\\')
                {
                    volumeName.Length--;
                }

                var volumeHandle = VirtualDiskNativeMethods.CreateFile(
                    volumeName.ToString(),
                    0,
                    VirtualDiskNativeMethods.FileShareModeFlags.FileShareRead |
                    VirtualDiskNativeMethods.FileShareModeFlags.FileShareWrite,
                    IntPtr.Zero,
                    VirtualDiskNativeMethods.CreationDispositionFlags.OpenExisting,
                    0,
                    IntPtr.Zero);
                if (volumeHandle == VirtualDiskNativeMethods.InvalidHandleValue)
                {
                    continue;
                }

                volumes.Add(volumeName.ToString());

                VirtualDiskNativeMethods.CloseHandle(volumeHandle);
            } while (VirtualDiskNativeMethods.FindNextVolume(findVolumeHandle, volumeName, volumeName.Capacity));

            VirtualDiskNativeMethods.FindVolumeClose(findVolumeHandle);

            return volumes;
        }
    }
}