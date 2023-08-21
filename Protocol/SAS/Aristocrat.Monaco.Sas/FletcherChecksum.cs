namespace Aristocrat.Monaco.Sas
{
    using System.Collections.Generic;

    /// <summary>
    ///     Provides extremely basic Fletcher style checksum for simple hashing or other data verification.
    /// </summary>
    public static class FletcherChecksum
    {
        /// <summary>
        ///     Takes a list of data and returns a 16-bit checksum.
        /// </summary>
        /// <param name="data">List of bytes representing the data you want a checksum for.</param>
        /// <returns>The checksum as a ushort.</returns>
        public static ushort Checksum16(IEnumerable<byte> data)
        {
            const uint byteMask = 0xFF;
            ushort sum1 = 0;
            ushort sum2 = 0;

            foreach (var nextByte in data)
            {
                sum1 = (ushort)((sum1 + nextByte) % byteMask);
                sum2 = (ushort)((sum2 + sum1) % byteMask);
            }

            return (ushort)((sum2 << 8) | sum1);
        }
    }
}
