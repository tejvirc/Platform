namespace Aristocrat.Monaco.Hardware.Contracts.VHD
{
    using Kernel;

    /// <summary>
    ///     Provides a mechanism to interact with a Virtual Hard Disk (VHD)
    /// </summary>
    public interface IVirtualDisk : IService
    {
        /// <summary>
        ///     Attaches a file to the specified path
        /// </summary>
        /// <param name="file">The VHD file to attach</param>
        /// <param name="path">The mount path for the image</param>
        /// <returns>the virtual disk file handle if successful</returns>
        VirtualDiskHandle AttachImage(string file, string path);

        /// <summary>
        ///     Unmounts an image
        /// </summary>
        /// <param name="handle">The VHD handle to detach</param>
        /// <param name="path">The mount path for the image</param>
        /// <returns>true upon success, otherwise false</returns>
        bool DetachImage(VirtualDiskHandle handle, string path);

        /// <summary>
        ///     Unmounts an image from the specified path
        /// </summary>
        /// <param name="file">The VHD file to detach</param>
        /// <param name="path">The mount path for the image</param>
        /// <returns>true upon success, otherwise false</returns>
        bool DetachImage(string file, string path);

        /// <summary>
        ///     Closes the VHD handle
        /// </summary>
        /// <param name="handle">The VHD handle to close</param>
        void Close(VirtualDiskHandle handle);
    }
}