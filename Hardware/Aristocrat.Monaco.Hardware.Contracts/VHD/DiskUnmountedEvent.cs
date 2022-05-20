namespace Aristocrat.Monaco.Hardware.Contracts.VHD
{
    using Kernel;

    /// <summary>
    ///     Published when a virtual disk (VHD, ISO) is unmounted
    /// </summary>
    public class DiskUnmountedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DiskUnmountedEvent" /> class.
        /// </summary>
        /// <param name="physicalPath">The physical path of the mount</param>
        public DiskUnmountedEvent(string physicalPath)
        {
            PhysicalPath = physicalPath;
        }

        /// <summary>
        ///     Gets the physical path of the mount
        /// </summary>
        public string PhysicalPath { get; }
    }
}