// <copyright file="MSCryptoSeedProvider.cs" company="Aristocrat, Inc.">
// Copyright (c) Aristocrat, Inc. All rights reserved.
// </copyright>

namespace PRNGLib
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Security.Cryptography;

    /// <summary>
    ///     MSCryptoSeedProvider
    /// </summary>
    public class MSCryptoSeedProvider : ISeedProvider
    {
        private readonly BinaryReader _reader;
        private readonly RNGCryptoServiceProvider _rng;
        private readonly MemoryStream _stream;

        private byte[] _bytes;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MSCryptoSeedProvider" /> class.
        /// </summary>
        public MSCryptoSeedProvider()
        {
            _rng = new RNGCryptoServiceProvider();
            _bytes = new byte[16];
            _stream = new MemoryStream(_bytes);
            _reader = new BinaryReader(_stream);
        }

        /// <summary>
        ///     GetValues
        /// </summary>
        /// <param name="values">Generated values.</param>
        /// <param name="numberOfValues">Num values to generate.</param>
        public void GetValues(Collection<uint> values, int numberOfValues)
        {
            int listLengthBytes = numberOfValues * 4;
            if (_bytes.Length < listLengthBytes)
            {
                _bytes = new byte[listLengthBytes];
            }

            _rng.GetBytes(_bytes);
            _stream.Position = 0;
            _stream.Write(_bytes, 0, listLengthBytes);
            _stream.Position = 0;

            values.Clear();
            for (int i = 0; i < numberOfValues; i++)
            {
                values.Add(_reader.ReadUInt32());
            }
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Dispose()
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _rng.Dispose();
                _reader.Dispose();
                _stream.Dispose();
            }
        }
    }
}