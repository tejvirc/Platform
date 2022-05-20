namespace Aristocrat.Monaco.Hardware.Contracts
{
    using System;
    using System.Collections.Generic;
    using Kernel;

    /// <summary>
    ///     States of the VPart 
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
    ///     A structure describing the virtual partition block in a secured partition
    /// </summary>
    public struct VirtualPartition
    {
        /// <summary>
        ///     Block contains the full bytes of the virtual partition block(all fields combined)
        /// </summary>
        public byte[] Block;

        /// <summary>
        ///     Name of the partition
        /// </summary>
        public string Name;

        /// <summary>
        ///     Hash of the partition
        /// </summary>
        public byte[] Hash;

        /// <summary>
        ///     Signature of the partition
        /// </summary>
        public byte[] Sig;

        /// <summary>
        ///     Source Partition (where the partition was initially extracted from)
        /// </summary>
        public int SourcePartition;

        /// <summary>
        ///     Source Offset (offset from where the partition was initially extracted from)
        /// </summary>
        public long SourceOffset;

        /// <summary>
        ///     Target Partition (where the partition is being written to)
        /// </summary>
        public int TargetPartition;

        /// <summary>
        ///     Size of partition
        /// </summary>
        public long Size;

        /// <summary>
        ///     Name of the file which held the partition data where the v-part was written
        /// </summary>
        public string SourceFile;

        /// <summary>
        ///     Current state
        /// </summary>
        public PartitionState State;
    }

    /// <summary>
    ///     Provides a mechanism to query the for configured hardware devices.
    /// </summary>
    public interface IOSService : IService
    {
        /// <summary>
        ///     Gets the version of the Operating System image
        /// </summary>
        Version OsImageVersion { get; }

        /// <summary>
        ///     Read the Virtual Partitions
        /// </summary>
        IList<VirtualPartition> VirtualPartitions { get; }
    }
}
