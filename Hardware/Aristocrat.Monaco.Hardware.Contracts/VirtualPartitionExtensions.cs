namespace Aristocrat.Monaco.Hardware.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     A set of <see cref="VirtualPartition" /> extensions
    /// </summary>
    public static class VirtualPartitionExtensions
    {
        /// <summary>
        ///     Gets the hash for the operating system
        /// </summary>
        /// <param name="this">A collection of partitions</param>
        /// <returns></returns>
        public static byte[] GetOperatingSystemHash(this IList<VirtualPartition> @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (@this.Count >= 2)
            {
                var bootPartition = @this.ElementAt(0);
                var osPartition = @this.ElementAt(1);

                if (bootPartition.Size > 0 && osPartition.Size > 0)
                {
                    return bootPartition.Hash.Concat(osPartition.Hash).ToArray();
                }
            }

            return new byte[0];
        }
    }
}