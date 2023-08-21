﻿namespace Aristocrat.Monaco.Common.Cryptography
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;

    /// <summary>
    ///     Implements a 32-bit CRC hash algorithm compatible with Zip etc.
    /// </summary>
    /// <remarks>
    ///     Crc32 should only be used for backward compatibility with older file formats
    ///     and algorithms. It is not secure enough for new applications.
    ///     If you need to call multiple times for the same data either use the HashAlgorithm
    ///     interface or remember that the result of one Compute call needs to be ~ (XOR) before
    ///     being passed in as the seed for the next Compute call.
    /// </remarks>
    public sealed class Crc32 : HashAlgorithm
    {
        private const uint DefaultPolynomial = 0xedb88320u;
        private const uint DefaultSeed = 0xffffffffu;

        private static uint[] _defaultTable;

        private readonly uint _seed;
        private readonly uint[] _table;
        private uint _hash;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Crc32" /> class.
        /// </summary>
        public Crc32()
            : this(DefaultPolynomial, DefaultSeed)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Crc32" /> class.
        /// </summary>
        /// <param name="polynomial">The polynomial</param>
        /// <param name="seed">The seed</param>
        public Crc32(uint polynomial, uint seed)
        {
            _table = InitializeTable(polynomial);
            _seed = _hash = seed;
        }

        /// <summary>
        ///     The hash size
        /// </summary>
        public override int HashSize => 32;

        /// <summary>
        ///     Initializes
        /// </summary>
        public override void Initialize()
        {
            _hash = _seed;
        }

        /// <summary>
        ///     Base hash method
        /// </summary>
        /// <param name="array"></param>
        /// <param name="ibStart"></param>
        /// <param name="cbSize"></param>
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            _hash = CalculateHash(_table, _hash, array, ibStart, cbSize);
        }

        /// <summary>
        ///     Finalizes the hash
        /// </summary>
        /// <returns></returns>
        protected override byte[] HashFinal()
        {
            var hashBuffer = UInt32ToBigEndianBytes(~_hash);
            HashValue = hashBuffer;
            return hashBuffer;
        }

        /// <summary>
        ///     Computes the hash of a buffer
        /// </summary>
        /// <param name="buffer">The buffer</param>
        /// <returns>A hash</returns>
        public static uint Compute(byte[] buffer)
        {
            return Compute(DefaultSeed, buffer);
        }

        /// <summary>
        ///     Computes the hash of a buffer
        /// </summary>
        /// <param name="seed">The seed</param>
        /// <param name="buffer">The buffer</param>
        /// <returns>A hash</returns>
        public static uint Compute(uint seed, byte[] buffer)
        {
            return Compute(DefaultPolynomial, seed, buffer);
        }

        /// <summary>
        ///     Computes the hash of a buffer
        /// </summary>
        /// <param name="polynomial">A polynomial</param>
        /// <param name="seed">The seed</param>
        /// <param name="buffer">The buffer</param>
        /// <returns>A hash</returns>
        public static uint Compute(uint polynomial, uint seed, byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
        }

        private static uint[] InitializeTable(uint polynomial)
        {
            if (polynomial == DefaultPolynomial && _defaultTable != null)
            {
                return _defaultTable;
            }

            var createTable = new uint[256];
            for (var i = 0; i < 256; i++)
            {
                var entry = (uint)i;
                for (var j = 0; j < 8; j++)
                {
                    if ((entry & 1) == 1)
                    {
                        entry = (entry >> 1) ^ polynomial;
                    }
                    else
                    {
                        entry = entry >> 1;
                    }
                }

                createTable[i] = entry;
            }

            if (polynomial == DefaultPolynomial)
            {
                _defaultTable = createTable;
            }

            return createTable;
        }

        private static uint CalculateHash(uint[] table, uint seed, IList<byte> buffer, int start, int size)
        {
            var hash = seed;
            for (var i = start; i < start + size; i++)
            {
                hash = (hash >> 8) ^ table[buffer[i] ^ (hash & 0xff)];
            }

            return hash;
        }

        private static byte[] UInt32ToBigEndianBytes(uint uint32)
        {
            var result = BitConverter.GetBytes(uint32);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(result);
            }

            return result;
        }
    }
}