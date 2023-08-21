namespace Aristocrat.Monaco.Asp.Client.Utilities
{
    using System;
    using System.Text;
    using Contracts;

    public class ByteArrayReader : IByteArrayReader
    {
        public ByteArrayReader(byte[] data, int startOffset = 0, int endOffset = -1)
        {
            Reset(data, startOffset, endOffset);
        }

        public byte[] Buffer { get; private set; }

        public int StartOffset { get; private set; }

        public int EndOffset { get; private set; }

        public int Position { get; set; }

        public int Length => EndOffset - StartOffset + 1;

        public int ReadInt32()
        {
            return (int)ReadNumeric(4);
        }

        public uint ReadUInt32()
        {
            return (uint)ReadNumeric(4);
        }

        public ushort ReadUInt16()
        {
            return (ushort)ReadNumeric(2);
        }

        public short ReadInt16()
        {
            return (short)ReadNumeric(2);
        }

        public byte ReadByte()
        {
            return (byte)ReadNumeric(1);
        }

        public string ReadString(int size)
        {
            if (Position + size > EndOffset)
            {
                throw new InvalidOperationException("Reached end of stream.");
            }

            var ret = Encoding.ASCII.GetString(Buffer, Position, size);
            Position += size;
            ret = ret.TrimEnd('\0');
            return ret;
        }

        public float ReadFloat()
        {
            return ReadNumeric(4);
        }

        public byte[] ReadBytes(int size)
        {
            if (Position + size > EndOffset)
            {
                throw new InvalidOperationException("Reached end of stream.");
            }

            var ret = new byte[size];
            Array.Copy(Buffer, Position, ret, 0, size);
            Position += size;
            return ret;
        }

        public byte[] ToArray()
        {
            var ret = new byte[EndOffset - StartOffset];
            Array.Copy(Buffer, StartOffset, ret, 0, ret.Length);
            return ret;
        }

        public void Reset(byte[] data, int startOffset = 0, int endOffset = -1)
        {
            Buffer = data ?? new byte[0];
            StartOffset = startOffset;
            Position = startOffset;
            EndOffset = endOffset >= 0 ? endOffset : Buffer.Length;
        }

        public static long ReadNumeric(byte[] buffer, int offset, int size)
        {
            if (buffer.Length <= offset + size && size > 8)
            {
                throw new ArgumentOutOfRangeException();
            }

            long value = 0;
            for (var i = offset; i < offset + size; i++)
            {
                value = (value << 8) | buffer[i];
            }

            return value;
        }

        private long ReadNumeric(int size)
        {
            if (Position + size > EndOffset)
            {
                throw new InvalidOperationException("Reached end of stream.");
            }

            long ret = 0;
            for (var i = 0; i < size; i++)
            {
                if (i > 0)
                {
                    ret <<= 8;
                }

                ret |= Buffer[Position++];
            }

            return ret;
        }
    }
}