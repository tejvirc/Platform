namespace Aristocrat.Monaco.Hardware.Serial.NoteAcceptor.ID003
{
    using System;
    using Protocols;

    /// <summary>
    /// 16-bit CCITT CRC with polynomial 1 + x^5 + x^12 + x^16 and seed of 0.
    /// </summary>
    public class Crc16Ccitt : ICrcEngine
    {
        private const uint Polynomial = 0x1081; // bit-reversed, shifted right 4 bits
        private const int Nibble = 0x0f; // half a byte
        private const int NibbleBitSize = 4; // half a byte

        /// <summary>
        ///     Construct
        /// </summary>
        public Crc16Ccitt()
        {
            Initialize(0);
        }

        /// <inheritdoc />
        public void Initialize(ushort seed)
        {
            Crc = seed;
        }

        /// <inheritdoc />
        public void Hash(byte[] bytes, uint start, uint count)
        {
            if (start > bytes.Length)
                throw new ArgumentOutOfRangeException();
            if (start + count > bytes.Length)
                throw new ArgumentOutOfRangeException();

            for (var index = start; index < (count + start); index++)
            {
                HashByte(bytes[index]);
            }
        }

        /// <summary>
        ///     Add another byte to the hash
        /// </summary>
        /// <param name="b">A byte</param>
        private void HashByte(byte b)
        {
            uint currentByte = b;
            uint quotient = (Crc ^ currentByte) & Nibble;
            Crc = (ushort)((Crc >> NibbleBitSize) ^ (quotient * Polynomial));
            quotient = (Crc ^ (currentByte >> NibbleBitSize)) & Nibble;
            Crc = (ushort)((Crc >> NibbleBitSize) ^ (quotient * Polynomial));
        }

        /// <inheritdoc />
        public ushort Crc { get; private set; }
    }
}
