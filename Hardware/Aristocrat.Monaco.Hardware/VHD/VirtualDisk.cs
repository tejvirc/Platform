namespace Aristocrat.Monaco.Hardware.VHD
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Contracts.VHD;
    using Kernel;
    using NativeDisk;
    using IVirtualDisk = Contracts.VHD.IVirtualDisk;

    /// <summary>
    ///     An <see cref="Contracts.VHD.IVirtualDisk" /> implementation
    /// </summary>
    public sealed class VirtualDisk : IVirtualDisk, IDisposable
    {
        private readonly NativeDisk.IVirtualDisk _virtualDisk;
        private readonly IEventBus _bus;
        private bool _disposed;

        public VirtualDisk()
            : this(ServiceManager.GetInstance().GetService<IEventBus>(),
                VirtualDiskFactory.CreateVirtualDisk())
        {
        }

        public VirtualDisk(IEventBus bus, NativeDisk.IVirtualDisk virtualDisk)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _virtualDisk = virtualDisk ?? throw new ArgumentNullException(nameof(virtualDisk));
            _virtualDisk.DiskMounted += VirtualDiskOnDiskMounted;
            _virtualDisk.DiskUnmounted += VirtualDiskOnDiskUnmounted;
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IVirtualDisk) };

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _virtualDisk.DiskMounted -= VirtualDiskOnDiskMounted;
            _virtualDisk.DiskUnmounted -= VirtualDiskOnDiskUnmounted;
            _disposed = true;
        }

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public SafeHandle AttachImage(string file, string path)
        {
            return _virtualDisk.AttachImage(file, path);
        }

        /// <inheritdoc />
        public bool DetachImage(SafeHandle handle, string path)
        {
            return _virtualDisk.DetachImage(handle, path);
        }

        /// <inheritdoc />
        public bool DetachImage(string file, string path)
        {
            return _virtualDisk.DetachImage(file, path);
        }

        /// <inheritdoc />
        public void Close(SafeHandle handle)
        {
            _virtualDisk.Close(handle);
        }

        private void VirtualDiskOnDiskUnmounted(object sender, DiskUnmountedEventArgs e)
        {
            _bus.Publish(new DiskUnmountedEvent(e.Path));
        }

        private void VirtualDiskOnDiskMounted(object sender, DiskMountedEventArgs e)
        {
            _bus.Publish(new DiskMountedEvent(e.VolumePath, e.FileName, e.Path));
        }
    }
}