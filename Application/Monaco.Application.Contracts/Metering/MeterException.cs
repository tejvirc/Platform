////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="MeterException.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 1996-2012 Video Gaming Technologies, Inc.  All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Thrown when there is a problem registering or removing a MeterProvider from the MeterManager.
    /// </summary>
    [Serializable]
    public class MeterException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MeterException" /> class.
        /// </summary>
        public MeterException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MeterException" /> class.
        /// </summary>
        /// <param name="message">The exception message</param>
        public MeterException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MeterException" /> class.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public MeterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MeterException" /> class.
        /// </summary>
        /// <param name="serializationInfo">The SerializationInfo</param>
        /// <param name="streamingContext">The StreamingContext</param>
        protected MeterException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}