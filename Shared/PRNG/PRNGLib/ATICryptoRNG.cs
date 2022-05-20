// <copyright file="ATICryptoRNG.cs" company="Aristocrat, Inc.">
// Copyright (c) Aristocrat, Inc. All rights reserved.
// </copyright>

namespace PRNGLib
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    ///     ATICryptoRNG, a port of the Gen7/Gen8 AES-crypto based RNG.
    /// </summary>
    public class ATICryptoRNG : IPRNG, IDisposable
    {
        private const int KeysizeBits = 128;
        private const int ElementBits = 128;
        private const int Elements64 = ElementBits / 64;
        private const int Elements32 = ElementBits / 32;
        private const int Elements16 = ElementBits / 16;
        private const int Elements8 = ElementBits / 8;
        private const int EntropyValsNeeded = KeysizeBits / 32;

        private const int MaxDrawsPerKey = 15000;

        private readonly Collection<uint> _entropyData = new Collection<uint>();

        private readonly object _generatorLock;

        private readonly GetTimeFunction _getTimeFunction;
        private readonly ulong _maxDrawsPerKey;
        private readonly PanicFunction _panicFunction;

        private readonly int _rngReady;

        private readonly IRandomStateProvider _stateProvider;

        private BinData _cipherText;
        private BinData _cryptoKey;

        private bool _disposed;

        private ulong _numDrawsWithCurrentKey;
        private BinData _plainText;
        private IRandomCryptoProvider _randomCryptoProvider;
        private ISeedProvider _seedProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ATICryptoRNG" /> class.
        /// </summary>
        /// <param name="stateProvider">An optional state provider</param>
        public ATICryptoRNG(IRandomStateProvider stateProvider)
        {
            _stateProvider = stateProvider;

            _rngReady = 0;

            _numDrawsWithCurrentKey = 0;

            _maxDrawsPerKey = MaxDrawsPerKey;
            _seedProvider = new MSCryptoSeedProvider();
            _randomCryptoProvider = new AESRandomCryptoProvider();
            _getTimeFunction = RNGUtil.GetCurrentMilliseconds;
            _panicFunction = RNGUtil.Panic;

            _plainText = new BinData(Elements8);
            _cipherText = new BinData(Elements8);
            _cryptoKey = new BinData(KeysizeBits / 8);

            _generatorLock = new object();

            if (!RestoreKey())
            {
                _panicFunction("Failed to restore RNG keys");
            }

            UpdateKey();

            var initialPlainText = new Collection<uint>();
            _seedProvider.GetValues(initialPlainText, Elements32);
            for (var currPtIndex = 0; currPtIndex < Elements32; ++currPtIndex)
            {
                _plainText.PutU32(currPtIndex, _plainText.GetU32(currPtIndex) ^ initialPlainText[currPtIndex]);
            }

            _rngReady = 1;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public ulong GetValue(ulong range)
        {
            ulong value;
            var limit = ulong.MaxValue / range * range;
            var width = limit / range;
            for (;;)
            {
                var num = DrawNumber();
                var result = num.GetU64(0);

                // Ensure that we don't draw from the end of the limit, which would skew the probability
                // slightly towards the bottom "half" of the range.
                if (result < limit)
                {
                    value = result / width;
                    break;
                }
            }

            return value;
        }

        /// <inheritdoc />
        public void GetValues(Collection<ulong> ranges, Collection<ulong> values)
        {
            values.Clear();
            var len = ranges.Count;
            for (var i = 0; i < len; i++)
            {
                values.Add(GetValue(ranges[i]));
            }
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        /// <param name="disposing">Indicates whether to release resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _cipherText.Dispose();
                _cryptoKey.Dispose();
                _plainText.Dispose();
                _seedProvider.Dispose();
                _randomCryptoProvider.Dispose();
            }

            _cipherText = null;
            _cryptoKey = null;
            _plainText = null;
            _seedProvider = null;
            _randomCryptoProvider = null;

            _disposed = true;
        }

        /// <summary>
        ///     Generate a 64-bit random number.
        /// </summary>
        /// <returns>BinData containing 8 bytes of random data.</returns>
        private BinData DrawNumber()
        {
            lock (_generatorLock)
            {
                // Checking _RNGReady value for validity.
                // Though it cannot programmatically be other than zero or 1, it's the last local module allocation above so
                //  memory corruption may affect it and this check can potentially notice and shut down.
                if (_rngReady != 1)
                {
                    _panicFunction("Invalid value for RNG initialization");
                }

                // update plaintext elements - first word reflects a time counter, strobing via xor
                _plainText.PutU32(0, _plainText.GetU32(0) ^ _getTimeFunction());

                // update plainText elements - second through fourth words are a simple 96-bit counter
                var p1 = _plainText.GetU32(1);
                ++p1;
                if (p1 == 0)
                {
                    var p2 = _plainText.GetU32(2);
                    ++p2;
                    if (p2 == 0)
                    {
                        var p3 = _plainText.GetU32(3);
                        ++p3;
                        _plainText.PutU32(3, p3);
                    }

                    _plainText.PutU32(2, p2);
                }

                _plainText.PutU32(1, p1);

                if (++_numDrawsWithCurrentKey >= _maxDrawsPerKey)
                {
                    _numDrawsWithCurrentKey = 0;
                    UpdateKey();
                }

                _randomCryptoProvider.Encrypt(_plainText.Data, _cipherText.Data);
            }

            return _cipherText;
        }

        private void UpdateKey()
        {
            // perturb the saved key by xoring in some entropy data from the seed provider
            _seedProvider.GetValues(_entropyData, EntropyValsNeeded);
            for (var currKeyIndex = 0; currKeyIndex < EntropyValsNeeded; ++currKeyIndex)
            {
                _cryptoKey.PutU32(currKeyIndex, _cryptoKey.GetU32(currKeyIndex) ^ _entropyData[currKeyIndex]);
            }

            // save the updated key
            if (!SaveKey())
            {
                _panicFunction("Failed to save RNG keys");
            }

            _randomCryptoProvider.SetKey(_cryptoKey.Data);
        }

        private bool SaveKey()
        {
            _stateProvider?.WriteKey(_cryptoKey.Data);

            return true;
        }

        private bool RestoreKey()
        {
            // GEN8 implementation fills key with 0's and returns true if key doesn't exist in persistent storage
            var data = _stateProvider?.ReadKey();
            if (data != null)
            {
                _cryptoKey.Data = data;
            }
            else
            {
                for (var i = 0; i < _cryptoKey.Data.Length; i++)
                {
                    _cryptoKey.Data[i] = 0;
                }
            }

            return true;
        }

        /// <summary>
        ///     GetTimeFunction
        /// </summary>
        /// <returns>uint milliseconds</returns>
        protected delegate uint GetTimeFunction();

        /// <summary>
        ///     Panic function.
        /// </summary>
        /// <param name="message">Message in thrown exception.</param>
        protected delegate void PanicFunction(string message);
    }
}