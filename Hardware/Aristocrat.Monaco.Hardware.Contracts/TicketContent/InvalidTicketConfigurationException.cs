////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="InvalidTicketConfigurationException.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 1996-2010 Video Gaming Technologies, Inc.  All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Hardware.Contracts.TicketContent
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Definition of the PropertyNotSetException class.
    /// </summary>
    [Serializable]
    public class InvalidTicketConfigurationException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidTicketConfigurationException" /> class.
        /// </summary>
        public InvalidTicketConfigurationException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidTicketConfigurationException" /> class.
        /// </summary>
        /// <param name="message">The exception message. </param>
        public InvalidTicketConfigurationException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidTicketConfigurationException" /> class.
        /// </summary>
        /// <param name="message">The exception message. </param>
        /// <param name="inner">The inner exception. </param>
        public InvalidTicketConfigurationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidTicketConfigurationException" /> class.
        /// </summary>
        /// <param name="info">The exception info. </param>
        /// <param name="context">The exception context. </param>
        protected InvalidTicketConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}