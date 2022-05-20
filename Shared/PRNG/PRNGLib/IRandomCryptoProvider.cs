// <copyright file="IRandomCryptoProvider.cs" company="Aristocrat, Inc.">
// Copyright (c) Aristocrat, Inc. All rights reserved.
// </copyright>

namespace PRNGLib
{
    using System;

    /// <summary>
    ///     IRandomCryptoProvider
    /// </summary>
    public interface IRandomCryptoProvider : IDisposable
    {
        /// <summary>
        ///     SetKey() - sets a new encryption key.
        /// </summary>
        /// <param name="cipherKey">The key.</param>
        void SetKey(byte[] cipherKey);

        /// <summary>
        ///     Encrypt() - encrypts plaintext using current encryption key.
        /// </summary>
        /// <param name="plainText">Input - plain text.</param>
        /// <param name="cipherText">Output - cipher text.</param>
        void Encrypt(byte[] plainText, byte[] cipherText);
    }
}