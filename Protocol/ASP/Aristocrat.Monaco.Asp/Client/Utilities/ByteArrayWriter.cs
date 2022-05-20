namespace Aristocrat.Monaco.Asp.Client.Utilities
{
    using System;
    using System.Text;
    using Contracts;

    public class ByteArrayWriter : IByteArrayWriter
    {
        public ByteArrayWriter(byte[] data = null, int startOffset = 0)
        {
            Reset(data, startOffset);
        }

        public byte[] Buffer { get; private set; }

        public int StartOffset { get; private set; }

        public int EndOffset => Buffer?.Length - 1 ?? 0;
        public int Length => Buffer?.Length - StartOffset ?? 0;

        public int Position { get; set; }

        public void Write(int value)
        {
            WriteNumeric(value, 4);
        }

        public void Write(uint value)
        {
            WriteNumeric(value, 4);
        }

        public void Write(short value)
        {
            WriteNumeric(value, 2);
        }

        public void Write(ushort value)
        {
            WriteNumeric(value, 2);
        }

        public void Write(byte value)
        {
            WriteNumeric(value, 1);
        }

        public void Write(string value, int size)
        {
            EnsureSizeAvailable(size);

            var ret = Encoding.ASCII.GetBytes(value, 0, Math.Min(value.Length, size), Buffer, Position);
            for (var i = ret; i < size; i++)
            {
                Buffer[Position + ret] = 0;
            }

            Position += size;
        }

        public void Write(float value)
        {
            WriteNumeric((long)value, 4);
        }

        public void Write(byte[] bytes, int offset = 0, int count = -1)
        {
            count = count < 0 ? bytes.Length - offset : count;
            EnsureSizeAvailable(count);
            Array.Copy(bytes, offset, Buffer, Position, count);
            Position += count;
        }

        private void EnsureSizeAvailable(int count)
        {
            if (Position + count > EndOffset + 1)
            {
                throw new IndexOutOfRangeException();
            }
        }

        public static void WriteNumeric(byte[] buffer, int offset, int size, long value)
        {
            if (buffer.Length <= offset + size - 1)
            {
                throw new IndexOutOfRangeException();
            }

            for (var i = 0; i < size; i++)
            {
                buffer[i + offset] = (byte)((value >> (8 * (size - i - 1))) & 0xFF);
            }
        }

        private void WriteNumeric(long value, int size)
        {
            WriteNumeric(Buffer, Position, size, value);
            Position += size;
        }

        public byte[] ToArray()
        {
            var ret = new byte[Position - StartOffset];
            Array.Copy(Buffer, StartOffset, ret, 0, ret.Length);
            return ret;
        }

        public void Reset(byte[] data, int startOffset = 0)
        {
            Buffer = data ?? new byte[0];
            StartOffset = startOffset;
            Position = startOffset;
        }
    }
}