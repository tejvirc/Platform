// <copyright file="BinData.cs" company="Aristocrat, Inc.">
// Copyright (c) Aristocrat, Inc. All rights reserved.
// </copyright>

namespace PRNGLib
{
    using System;
    using System.IO;

    /// <summary>
    ///     byte array addressable as u64,u32,u8
    ///     Substitute for C++ union of arrays, without
    ///     headache of fixed buffers / pin to pointers.
    /// </summary>
    internal class BinData : IDisposable
    {
        private readonly byte[] _data;
        private readonly BinaryReader _reader;
        private readonly MemoryStream _stream;
        private readonly BinaryWriter _writer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BinData" /> class.
        /// </summary>
        /// <param name="sizeInBytes">Size in bytes.</param>
        public BinData(int sizeInBytes)
        {
            _data = new byte[sizeInBytes];
            _stream = new MemoryStream(_data);
            _reader = new BinaryReader(_stream);
            _writer = new BinaryWriter(_stream);
        }

        /// <summary>
        ///     Gets or sets data as byte array.
        /// </summary>
        public byte[] Data
        {
            get { return _data; }

            set
            {
                for (var i = 0; i < value.Length; i++)
                {
                    _data[i] = value[i];
                }
            }
        }

        /// <summary>
        ///     Dipose() - release resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     GetU64()
        /// </summary>
        /// <param name="index">Index of 64-bit int to return.</param>
        /// <returns>Specified 64-bit quantity.</returns>
        public UInt64 GetU64(int index)
        {
            _stream.Position = index << 3;
            return _reader.ReadUInt64();
        }

        /// <summary>
        ///     PutU64()
        /// </summary>
        /// <param name="index">Index of 64-bit to write.</param>
        /// <param name="value">Value to write.</param>
        public void PutU64(int index, UInt64 value)
        {
            _stream.Position = index << 3;
            _writer.Write(value);
        }

        /// <summary>
        ///     GetU32()
        /// </summary>
        /// <param name="index">Index of 32-bit int to return.</param>
        /// <returns>Specified 32-bit quantity.</returns>
        public UInt32 GetU32(int index)
        {
            _stream.Position = index << 2;
            return _reader.ReadUInt32();
        }

        /// <summary>
        ///     PutU32()
        /// </summary>
        /// <param name="index">Index of 32-bit to write.</param>
        /// <param name="value">Value to write.</param>
        public void PutU32(int index, UInt32 value)
        {
            _stream.Position = index << 2;
            _writer.Write(value);
        }

        /// <summary>
        ///     GetU8()
        /// </summary>
        /// <param name="index">Index of 8-bit int to return.</param>
        /// <returns>Specified 8-bit quantity.</returns>
        public Byte GetU8(int index)
        {
            _stream.Position = index;
            return (byte)_stream.ReadByte();
        }

        /// <summary>
        ///     PutU8()
        /// </summary>
        /// <param name="index">Index of 8-bit to write.</param>
        /// <param name="value">Value to write.</param>
        public void PutU8(int index, Byte value)
        {
            _stream.Position = index;
            _writer.Write(value);
        }

        /// <summary>
        ///     Dispose()
        /// </summary>
        /// <param name="disposing">Indicates whether to release resources.</param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                _reader.Dispose();
                _writer.Dispose();
                _stream.Dispose();
            }
        }
    }
}