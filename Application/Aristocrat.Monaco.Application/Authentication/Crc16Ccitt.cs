namespace Aristocrat.Monaco.Application.Authentication
{
    using System;
    using System.Security.Cryptography;

    /// <summary>
    ///     This class is used for generating a CRC 16 CCITT
    /// </summary>
    internal class Crc16Ccitt : HashAlgorithm
    {
        private const ushort Nibble = 0xf;
        private const ushort Polynomial = 0x1081;

        /// <summary>
        ///     Creates a CRC 16 CCITT instance
        /// </summary>
        /// <param name="seed"></param>
        public Crc16Ccitt(ushort seed) => Crc = seed;

        /// <summary>
        ///     Get the CRC.
        /// </summary>
        public ushort Crc { get; private set; }

        /// <inheritdoc />
        public override byte[] Hash => BitConverter.GetBytes(Crc);

        /// <summary>
        ///     Add another byte to the hash
        /// </summary>
        /// <param name="b">A byte</param>
        public void HashByte(byte b)
        {
            uint currentByte = b;
            var quotient = (Crc ^ currentByte) & Nibble;
            Crc = (ushort)((Crc >> 4) ^ (quotient * Polynomial));
            quotient = (Crc ^ (currentByte >> 4)) & Nibble;
            Crc = (ushort)((Crc >> 4) ^ (quotient * Polynomial));
        }

        /// <inheritdoc />
        public override void Initialize()
        {
        }

        /// <inheritdoc />
        protected override void HashCore(byte[] array, int start, int count)
        {
            for (var index = start; index < count; index++)
            {
                HashByte(array[index]);
            }
        }

        /// <inheritdoc />
        protected override byte[] HashFinal()
        {
            return BitConverter.GetBytes(Crc);
        }
    }
}