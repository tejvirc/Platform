////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="NoteAcceptorCoordinatingException.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 2011-2012 Video Gaming Technologies, Inc.  All rights reserved.
// Confidential and proprietary information.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.NoteAcceptor
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Definition of the NoteAcceptorCoordinatingException class.
    /// </summary>
    [Serializable]
    public class NoteAcceptorCoordinatingException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorCoordinatingException" /> class.
        /// </summary>
        public NoteAcceptorCoordinatingException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorCoordinatingException" /> class and initializes
        ///     the contained message.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        public NoteAcceptorCoordinatingException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorCoordinatingException" /> class.
        ///     Initializes a new instance of the NoteAcceptorCoordinatingException class and initializes
        ///     the contained message and inner exception reference.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        /// <param name="inner">Exception to set as InnerException.</param>
        public NoteAcceptorCoordinatingException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorCoordinatingException" /> class.
        ///     Initializes a new instance of the NoteAcceptorCoordinatingException class with serialized data.
        /// </summary>
        /// <param name="info">Information on how to serialize a VgtTransactionDispatchingException.</param>
        /// <param name="context">Information on the streaming context for a VgtTransactionDispatchingException.</param>
        protected NoteAcceptorCoordinatingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}