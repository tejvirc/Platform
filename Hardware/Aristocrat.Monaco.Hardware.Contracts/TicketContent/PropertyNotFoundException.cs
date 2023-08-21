////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="PropertyNotFoundException.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 1996-2010 Video Gaming Technologies, Inc.  All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Hardware.Contracts.TicketContent
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Definition of the PropertyNotFoundException class.
    /// </summary>
    [Serializable]
    public class PropertyNotFoundException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyNotFoundException" /> class.
        /// </summary>
        public PropertyNotFoundException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyNotFoundException" /> class.
        /// </summary>
        /// <param name="message">The exception message. </param>
        public PropertyNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyNotFoundException" /> class.
        /// </summary>
        /// <param name="message">The exception message. </param>
        /// <param name="inner">The inner exception. </param>
        public PropertyNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyNotFoundException" /> class.
        /// </summary>
        /// <param name="info">The exception info. </param>
        /// <param name="context">The exception context. </param>
        protected PropertyNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}