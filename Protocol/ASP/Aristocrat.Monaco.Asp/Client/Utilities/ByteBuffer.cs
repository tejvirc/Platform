namespace Aristocrat.Monaco.Asp.Client.Utilities
{
    using System;

    public class ByteBuffer
    {
        private int _end;

        public ByteBuffer(byte[] data, int start = 0, int end = -1)
        {
            Reset(data, start, end);
        }

        public ByteBuffer(ByteBuffer data, int start = 0, int end = -1)
        {
            Reset(data, start, end);
        }

        public byte[] Data { get; private set; }

        public int Length => _end - Offset + 1;
        public int Offset { get; private set; }

        public byte this[int index]
        {
            get => Data[ToDataIndex(index)];
            set => Data[ToDataIndex(index)] = value;
        }

        private int ToDataIndex(int index)
        {
            return Offset + index;
        }

        public void Reset(ByteBuffer data, int start = 0, int end = -1)
        {
            end = end < 0 ? data.Length - 1 : end;
            Reset(data.Data, data.ToDataIndex(start), end);
        }

        public void Reset(byte[] data, int start = 0, int end = -1)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }

            Offset = start;
            if (Offset >= data.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            _end = end < 0 ? data.Length - start - 1 : end;
            if (_end >= data.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            Data = data;
        }

        public void Clear()
        {
            Array.Clear(Data, Offset, Length);
        }
    }
}