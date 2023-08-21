namespace Aristocrat.Monaco.Asp.Client.Comms
{
    using Utilities;

    /// <summary>
    ///     Class that encapsulates ASP data link layer packet.
    ///     ---------------------------------------------------------------------
    ///     | MAC 1 byte  |Length 1 byte  |Data Up to 254 (2n) bytes |CRC 2 bytes |
    ///     ---------------------------------------------------------------------
    /// </summary>
    public class DataLinkPacket
    {
        private ByteBuffer _payload;

        public DataLinkPacket()
        {
            Mac = 1;
            Sequence = 0;
            DataLength = 0;
            Crc = 0;
        }

        public bool Valid { get; private set; }
        public ByteBuffer Buffer { get; } = new ByteBuffer(new byte[254]);

        public ByteBuffer Payload
        {
            get
            {
                if (_payload == null)
                {
                    _payload = new ByteBuffer(Buffer.Data);
                }

                _payload.Reset(Buffer.Data, 2, 2 + DataLength - 1);
                return _payload;
            }
        }

        public byte[] Bytes => Buffer.Data;

        /// <summary>
        ///     MAC	This contains the frame sequencing and other related information. It can also be used as a start of frame
        ///     identifier.
        ///     The MAC uniquely identifies each message. It consists of the following fields:
        ///     •	Type - 2 bits
        ///     •	Sequence - 3 bits
        ///     Type
        ///     The following types of ASP TP layer frame are currently defined:
        ///     •	0x00	Test
        ///     •	0x01	ASP (5000 &amp; 6000)
        ///     •	0x02	Reserved
        ///     •	0x03	Reserved
        /// </summary>
        public byte Mac
        {
            get => (byte)((Buffer[0] >> 6) & 0x3);
            private set => Buffer[0] = (byte)(((value & 0x3) << 6) | ((Sequence & 0x7) << 3));
        }

        /// <summary>
        ///     Sequence - 3 bits
        /// </summary>
        public int Sequence
        {
            get => (Buffer[0] >> 3) & 0x7;
            private set => Buffer[0] = (byte)(((Mac & 0x3) << 6) | ((value & 0x7) << 3));
        }

        /// <summary>
        ///     Data Length	Number of data bytes in frame.
        /// </summary>
        public int DataLength
        {
            get => Buffer[1];
            set => Buffer[1] = (byte)(value + value % 2);
        }

        /// <summary>
        ///     Total number of bytes in the packet including data and CRC.
        /// </summary>
        public int Length => DataLength + DataLength % 2 + 4;

        /// <summary>
        ///     Current position of next byte in the packet.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        ///     Number of more bytes to be read for the packet.
        /// </summary>
        public int NumberOfBytesToRead
        {
            get
            {
                if (Position == 0 || Position == 1)
                {
                    return 1;
                }

                if (Complete)
                {
                    return 0;
                }

                return Length - Position;
            }
        }

        /// <summary>
        ///     True if the packet has all the bytes.
        /// </summary>
        public bool Complete => Position == Length;

        /// <summary>
        ///     CRC-16 check sum of all fields.
        /// </summary>
        public ushort Crc
        {
            get => (ushort)(Buffer[Length - 1] | (Buffer[Length - 2] << 8));
            set
            {
                Buffer[Length - 1] = (byte)(value & 0xFF);
                Buffer[Length - 2] = (byte)((value >> 8) & 0xFF);
            }
        }

        /// <summary>
        ///     Checks if the packet is complete and valid after receiving new bytes.
        /// </summary>
        /// <param name="newBytesRead">New bytes that have been read into the buffer.</param>
        /// <param name="expectedSequence">Expected sequence of this packet.</param>
        /// <returns></returns>
        internal bool CheckIfComplete(int newBytesRead, int expectedSequence)
        {
            Position += newBytesRead;
            if (Position > 0 &&
                (Mac != 1 || Sequence != 0 && Sequence != expectedSequence && Sequence != (expectedSequence + 1) % 8) ||
                Position > 1 && DataLength % 2 != 0 ||
                Complete && AspCrc.CalcCrc(Bytes, 0, Length) != 0
            )
            {
                Position = 0;
            }
            else if (Complete)
            {
                Valid = true;
            }

            return Valid;
        }

        /// <summary>
        ///     Resets the packet as if no bytes have been received.
        /// </summary>
        internal void Reset()
        {
            Position = 0;
            Buffer.Clear();
            Valid = false;
        }

        /// <summary>
        ///     Updates the CRC field of the packet.
        /// </summary>
        /// <param name="sequence">Sequence number of the packet.</param>
        internal void UpdateCrc(int sequence)
        {
            Mac = 1;
            Sequence = sequence;
            Crc = AspCrc.CalcCrc(Bytes, 0, Length - 2);
        }
    }
}