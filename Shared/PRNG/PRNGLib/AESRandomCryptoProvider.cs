// <copyright file="AESRandomCryptoProvider.cs" company="Aristocrat, Inc.">
// Copyright (c) Aristocrat, Inc. All rights reserved.
// </copyright>

namespace PRNGLib
{
    using System;
    using System.Security.Cryptography;

    /// <summary>
    ///     AESRandomCryptoProvider
    /// </summary>
    public class AESRandomCryptoProvider : IRandomCryptoProvider, IDisposable
    {
        private AesCryptoServiceProvider _cryptoService;
        private ICryptoTransform _encryptor;

        private byte[] _key;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AESRandomCryptoProvider" /> class.
        ///     Constructor
        /// </summary>
        public AESRandomCryptoProvider()
        {
            _cryptoService = new AesCryptoServiceProvider();
            CreateEncryptor();
        }

        /// <summary>
        ///     Encrypt - encrypts plaintext.
        /// </summary>
        /// <param name="plainText">Input plain text.</param>
        /// <param name="cipherText">Output cipher text.</param>
        public void Encrypt(byte[] plainText, byte[] cipherText)
        {
            // Expecting cipher text to be same length as plain text.
            // Since our plain text is the size of a block, full AES
            // encryption would result in an extra block for padding.
            // Instead, just use TransformBlock() which doesn't pad.
            _encryptor.TransformBlock(plainText, 0, plainText.Length, cipherText, 0);
        }

        /// <summary>
        ///     Set a new cryptographic key.
        /// </summary>
        /// <param name="cipherKey">The new cryptographic key.</param>
        public void SetKey(byte[] cipherKey)
        {
            int bytes = cipherKey.Length;
            _key = new byte[bytes];
            cipherKey.CopyTo(_key, 0);

            _cryptoService.Key = _key;

            // Create new Encryptor with new key.
            CreateEncryptor();
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
        ///     Dispose
        /// </summary>
        /// <param name="disposing">Whether to dispose unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_encryptor != null)
                {
                    _encryptor.Dispose();
                    _encryptor = null;
                }

                if (_cryptoService != null)
                {
                    _cryptoService.Dispose();
                    _cryptoService = null;
                }
            }
        }

        private void CreateEncryptor()
        {
            _encryptor?.Dispose();
            _encryptor = _cryptoService.CreateEncryptor();
        }
    }
}