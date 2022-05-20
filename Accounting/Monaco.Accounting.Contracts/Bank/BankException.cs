////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="BankException.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 2012 Video Gaming Technologies, Inc.  All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Throw when there is a problem depositing/withdrawing from the Bank.
    /// </summary>
    [Serializable]
    public class BankException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BankException" /> class.
        /// </summary>
        public BankException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BankException" /> class.
        /// </summary>
        /// <param name="message">The exception message</param>
        public BankException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BankException" /> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public BankException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BankException" /> class.
        /// </summary>
        /// <param name="serializationInfo">The SerializationInfo.</param>
        /// <param name="streamingContext">The StreamingContext.</param>
        protected BankException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}