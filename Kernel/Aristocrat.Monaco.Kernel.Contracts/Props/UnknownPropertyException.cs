namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Definition of the UnknownPropertyException class.  This exception should be
    ///     thrown whenever an IPropertyProvider implementation's GetProperty() or
    ///     SetProperty() method is called, the property name parameter is not
    ///     recognized by the provider, and it has no mechanism for handling such a
    ///     situation.
    /// </summary>
    [Serializable]
    public class UnknownPropertyException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UnknownPropertyException" /> class.
        /// </summary>
        public UnknownPropertyException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnknownPropertyException" /> class.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        public UnknownPropertyException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnknownPropertyException" /> class.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        /// <param name="inner">Exception to set as InnerException.</param>
        public UnknownPropertyException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnknownPropertyException" /> class.
        /// </summary>
        /// <param name="info">Information on how to serialize a UnknownPropertyException.</param>
        /// <param name="context">Information on the streaming context for a UnknownPropertyException.</param>
        protected UnknownPropertyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}