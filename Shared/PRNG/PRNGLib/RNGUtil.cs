// <copyright file="RNGUtil.cs" company="Aristocrat, Inc.">
// Copyright (c) Aristocrat, Inc. All rights reserved.
// </copyright>

namespace PRNGLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    /// <summary>
    ///     RNGUtil - External functions used by ATICryptoRNG.
    /// </summary>
    public static class RNGUtil
    {
        /// <summary>
        ///     GetCurrentMilliseconds
        /// </summary>
        /// <returns>Time in milliseconds since system start.</returns>
        public static UInt32 GetCurrentMilliseconds()
        {
            // C++ code truncates this to 32-bit. ?
            return (UInt32)GetTickCount64();
        }

        /// <summary>
        ///     Panic - throws an exception.
        /// </summary>
        /// <param name="message">Message for exception.</param>
        public static void Panic(string message)
        {
            throw new ArgumentException(message);
        }

        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "specific to this class")]
        [DllImport("kernel32")]
        private static extern UInt64 GetTickCount64();
    }
}