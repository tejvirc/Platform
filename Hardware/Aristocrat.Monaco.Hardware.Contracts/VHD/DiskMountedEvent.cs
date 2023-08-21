namespace Aristocrat.Monaco.Hardware.Contracts.VHD
{
    using Kernel;

    /// <summary>
    ///     Published when a virtual disk (VHD, ISO) is mounted
    /// </summary>
    public class DiskMountedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DiskMountedEvent" /> class.
        /// </summary>
        /// <param name="volumeName">The volume GUID path</param>
        /// <param name="virtualDisk">The mounted file</param>
        /// <param name="physicalPath">The physical path of the mount</param>
        public DiskMountedEvent(string volumeName, string virtualDisk, string physicalPath)
        {
            VolumeName = volumeName;
            VirtualDisk = virtualDisk;
            PhysicalPath = physicalPath;
        }

        /// <summary>
        ///     Gets the volume GUID path
        /// </summary>
        public string VolumeName { get; }

        /// <summary>
        ///     Gets the mounted file
        /// </summary>
        public string VirtualDisk { get; }

        /// <summary>
        ///     Gets the physical path of the mount
        /// </summary>
        public string PhysicalPath { get; }
    }
}