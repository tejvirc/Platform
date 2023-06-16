namespace Aristocrat.Monaco.Hardware.Contracts
{
    using System;
    using System.Collections.Generic;
    using Kernel;
    using NativeOS.Services.OS;

    /// <summary>
    ///     States of the Virtual Partition
    /// </summary>
    public enum PartitionState
    {
        /// <summary>
        ///     Inactive
        /// </summary>
        Inactive = 0,

        /// <summary>
        ///     Active
        /// </summary>
        Active = 0x1,

        /// <summary>
        ///     Old Active
        /// </summary>
        OldActive = 0x101
    }

    /// <summary>
    ///     Provides a mechanism to query the for configured hardware devices.
    /// </summary>
    [CLSCompliant(false)]
    public interface IOSService : IService
    {
        /// <summary>
        ///     Gets the version of the Operating System image
        /// </summary>
        Version OsImageVersion { get; }

        /// <summary>
        ///     Read the Virtual Partitions
        /// </summary>
        IReadOnlyCollection<VirtualPartition> VirtualPartitions { get; }

        /// <summary>
        ///     Gets the operating system hash
        /// </summary>
        IEnumerable<byte> GetOperatingSystemHash();
    }
}
