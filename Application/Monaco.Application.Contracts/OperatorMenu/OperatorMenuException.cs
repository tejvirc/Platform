////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="OperatorMenuException.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 2009-2010 Video Gaming Technologies, Inc.  All rights reserved.
// Confidential and proprietary information.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Vgt.Client12.Application.OperatorMenu
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Definition of the OperatorMenuException class.  This exception should be
    ///     thrown whenever the operator menu encounters a fatal error.
    /// </summary>
    [Serializable]
    public class OperatorMenuException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorMenuException" /> class.
        /// </summary>
        public OperatorMenuException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorMenuException" /> class.
        ///     Initializes a new instance of the OperatorMenuException class and initializes
        ///     the contained message.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        public OperatorMenuException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorMenuException" /> class.
        ///     Initializes a new instance of the OperatorMenuException class and initializes
        ///     the contained message and inner exception reference.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        /// <param name="inner">Exeception to set as InnerException.</param>
        public OperatorMenuException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorMenuException" /> class.
        ///     Initializes a new instance of the OperatorMenuException class with serialized data.
        /// </summary>
        /// <param name="info">Information on how to serialize an ServiceException.</param>
        /// <param name="context">Information on the streaming context for an OperatorMenuException.</param>
        protected OperatorMenuException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}