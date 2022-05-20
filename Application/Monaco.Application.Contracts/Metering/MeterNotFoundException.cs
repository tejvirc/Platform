////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="MeterNotFoundException.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 1996-2012 Video Gaming Technologies, Inc.  All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Thrown when a meter is not found in the MeterManager.
    /// </summary>
    [Serializable]
    public class MeterNotFoundException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MeterNotFoundException" /> class.
        /// </summary>
        public MeterNotFoundException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MeterNotFoundException" /> class.
        /// </summary>
        /// <param name="message">The exception message</param>
        public MeterNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MeterNotFoundException" /> class.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public MeterNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MeterNotFoundException" /> class.
        /// </summary>
        /// <param name="serializationInfo">The SerializationInfo</param>
        /// <param name="streamingContext">The StreamingContext</param>
        protected MeterNotFoundException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}