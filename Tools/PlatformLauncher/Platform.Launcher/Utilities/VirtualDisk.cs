namespace Platform.Launcher.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class VirtualDisk
    {
        public VirtualDiskHandle AttachImage(string file, string path)
        {
            var attachParameters = new VirtualDiskNativeMethods.AttachVirtualDiskParameters
            {
                version = VirtualDiskNativeMethods.AttachVirtualDiskVersion.AttachVirtualDiskVersion1
            };

            var before = GetVolumes();

            var handle = Open(file, VirtualDiskNativeMethods.VirtualDiskAccessMask.VirtualDiskAccessAttachRo | VirtualDiskNativeMethods.VirtualDiskAccessMask.VirtualDiskAccessGetInfo);
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
                Console.WriteLine($"Failed to attach virtual disk {file} - Error {Marshal.GetLastWin32Error()}");

                handle.Close();
                handle.SetHandleAsInvalid();

                return handle;
            }

            var after = GetVolumes();

            var volumePath = after.FirstOrDefault(v => before.All(v2 => v != v2));

            if (!MountToDriveLetter(volumePath, path))
            {
                handle.Close();
                handle.SetHandleAsInvalid();

                return handle;
            }

            return handle;
        }

        public bool DetachImage(VirtualDiskHandle handle, string path)
        {
            if (VirtualDiskNativeMethods.DetachVirtualDisk(
                    handle,
                    VirtualDiskNativeMethods.DetachVirtualDiskFlag.DetachVirtualDiskFlagNone,
                    0) != VirtualDiskNativeMethods.Success)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());

                return false;
            }

            return VirtualDiskNativeMethods.DeleteVolumeMountPoint(path);
        }

        public bool DetachImage(string file, string path)
        {
            var handle = Open(file, VirtualDiskNativeMethods.VirtualDiskAccessMask.VirtualDiskAccessDetach);

            if (handle == null)
            {
                return false;
            }

            if (!DetachImage(handle, path))
            {
                handle.Close();
                return false;
            }

            handle.Close();
            return true;
        }

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
                Console.WriteLine($"Failed to open virtual disk {file} - Error {Marshal.GetLastWin32Error()}");
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

            if (!VirtualDiskNativeMethods.SetVolumeMountPoint(mountPoint, vhdVolumePath))
            {
                Console.WriteLine($"Failed to mount {mountPoint} to volume {vhdVolumePath} - Error {Marshal.GetLastWin32Error()}");

                return false;
            }

            return true;
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
                    VirtualDiskNativeMethods.FileShareModeFlags.FileShareRead | VirtualDiskNativeMethods.FileShareModeFlags.FileShareWrite,
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
            }
            while (VirtualDiskNativeMethods.FindNextVolume(findVolumeHandle, volumeName, volumeName.Capacity));

            VirtualDiskNativeMethods.FindVolumeClose(findVolumeHandle);

            return volumes;
        }
    }
}