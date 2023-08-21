namespace Aristocrat.Monaco.Protocol.Common.Communication
{
    using System;
    using System.Text;

    /// <summary>
    ///     Dynamic byte buffer.
    /// </summary>
    public class Buffer
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Buffer"/> class.
        /// </summary>
        public Buffer()
        {
            Data = new byte[0];
            Size = 0;
            Offset = 0;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Buffer"/> class.
        /// </summary>
        /// <param name="capacity"></param>
        public Buffer(long capacity)
        {
            Data = new byte[capacity];
            Size = 0;
            Offset = 0;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Buffer"/> class.
        /// </summary>
        /// <param name="data"></param>
        public Buffer(byte[] data)
        {
            Data = data;
            Size = data.Length;
            Offset = 0;
        }

        /// <summary>
        ///     Gets a value that indicates whether the buffer is empty.
        /// </summary>
        public bool IsEmpty => Data == null || Size == 0;

        /// <summary>
        ///     Gets or sets bytes memory buffer.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        ///     Gets the bytes memory buffer capacity.
        /// </summary>
        public long Capacity => Data.Length;

        /// <summary>
        ///     Gets or sets the bytes memory buffer size.
        /// </summary>
        public long Size { get; private set; }

        /// <summary>
        ///     Gets or sets the bytes memory buffer offset.
        /// </summary>
        public long Offset { get; private set; }

        /// <summary>
        ///     Gets or sets the buffer indexer operator.
        /// </summary>
        public byte this[int index] => Data[index];

        /// <summary>
        ///     Clear the current buffer and its offset.
        /// </summary>
        public void Clear()
        {
            Size = 0;
            Offset = 0;
        }

        /// <summary>
        ///     Extract the string from buffer of the given offset and size.
        /// </summary>
        public string ExtractString(long offset, long size)
        {
            if (offset + size > Size)
            {
                throw new ArgumentException(@"Invalid offset and size", nameof(offset));
            }

            return Encoding.UTF8.GetString(Data, (int)offset, (int)size);
        }

        /// <summary>
        ///     Remove the buffer of the given offset and size.
        /// </summary>
        public void Remove(long offset, long size)
        {
            if (offset + size > Size)
            {
                throw new ArgumentException(@"Invalid offset and size", nameof(offset));
            }

            Array.Copy(Data, offset + size, Data, offset, Size - size - offset);

            Size -= size;

            if (Offset >= offset + size)
            {
                Offset -= size;
            }
            else if (Offset >= offset)
            {
                Offset -= Offset - offset;

                if (Offset > Size)
                {
                    Offset = Size;
                }
            }
        }

        /// <summary>
        ///     Reserve the buffer of the given capacity.
        /// </summary>
        public void Reserve(long capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentException(@"Invalid reserve capacity", nameof(capacity));
            }

            if (capacity <= Capacity)
            {
                return;
            }

            var data = new byte[Math.Max(capacity, 2 * Capacity)];

            Array.Copy(Data, 0, data, 0, Size);

            Data = data;
        }

        /// <summary>
        ///     Resize the current buffer.
        /// </summary>
        /// <param name="size"></param>
        public void Resize(long size)
        {
            Reserve(size);
            Size = size;
            if (Offset > Size)
                Offset = Size;
        }

        /// <summary>
        ///     Shift the current buffer offset.
        /// </summary>
        /// <param name="offset"></param>
        public void Shift(long offset) => Offset += offset;

        /// <summary>
        ///     Unshift the current buffer offset.
        /// </summary>
        /// <param name="offset"></param>
        public void Unshift(long offset) => Offset -= offset;

        /// <summary>
        ///     Append the given buffer.
        /// </summary>
        /// <param name="buffer">Buffer to append.</param>
        /// <returns>Count of append bytes.</returns>
        public long Append(byte[] buffer)
        {
            Reserve(Size + buffer.Length);
            Array.Copy(buffer, 0, Data, Size, buffer.Length);
            Size += buffer.Length;
            return buffer.Length;
        }

        /// <summary>
        ///     Append the given buffer fragment.
        /// </summary>
        /// <param name="buffer">Buffer to append.</param>
        /// <param name="offset">Buffer offset.</param>
        /// <param name="size">Buffer size.</param>
        /// <returns>Count of append bytes.</returns>
        public long Append(byte[] buffer, long offset, long size)
        {
            Reserve(Size + size);
            Array.Copy(buffer, offset, Data, Size, size);
            Size += size;
            return size;
        }

        /// <summary>
        ///     Append the given text in UTF-8 encoding.
        /// </summary>
        /// <param name="text">Text to append.</param>
        /// <returns>Count of append bytes.</returns>
        public long Append(string text)
        {
            Reserve(Size + Encoding.UTF8.GetMaxByteCount(text.Length));
            long result = Encoding.UTF8.GetBytes(text, 0, text.Length, Data, (int)Size);
            Size += result;
            return result;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ExtractString(0, Size);
        }
    }
}
