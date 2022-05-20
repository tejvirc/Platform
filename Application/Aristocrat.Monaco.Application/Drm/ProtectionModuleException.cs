namespace Aristocrat.Monaco.Application.Drm
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class ProtectionModuleException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProtectionModuleException" /> class.
        /// </summary>
        public ProtectionModuleException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProtectionModuleException" /> class.
        ///     Initializes a new instance of the ProtectionModuleException class and initializes
        ///     the contained message.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        public ProtectionModuleException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProtectionModuleException" /> class and initializes
        ///     the contained message and inner exception reference.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        /// <param name="inner">Exception to set as InnerException.</param>
        public ProtectionModuleException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProtectionModuleException" /> class with serialized data.
        /// </summary>
        /// <param name="info">Information on how to serialize a ProtectionModuleException.</param>
        /// <param name="context">Information on the streaming context for a ProtectionModuleException.</param>
        protected ProtectionModuleException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}