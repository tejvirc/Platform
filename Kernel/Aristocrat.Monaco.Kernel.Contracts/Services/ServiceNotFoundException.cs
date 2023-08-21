namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    ///     This Exception gets thrown when the ServiceManager could not find a particular IService implementation.
    /// </summary>
    [Serializable]
    public class ServiceNotFoundException : ServiceException
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceNotFoundException" /> class.
        /// </summary>
        public ServiceNotFoundException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceNotFoundException" /> class. Contains the type.
        /// </summary>
        /// <param name="type">The type of the service not found.</param>
        public ServiceNotFoundException(Type type)
            : base(string.Format(CultureInfo.InvariantCulture, "Service of type {0} was not found.", type))
        {
            ServiceType = type;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceNotFoundException" /> class. Contains the type.
        /// </summary>
        /// <param name="type">The type of the service not found.</param>
        /// <param name="inner">Exception to set as InnerException.</param>
        public ServiceNotFoundException(Type type, Exception inner)
            : base(string.Format(CultureInfo.InvariantCulture, "Service of type {0} was not found.", type), inner)
        {
            ServiceType = type;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceNotFoundException" /> class.  Also contains an error
        ///     information message.
        /// </summary>
        /// <param name="message">Associated error information for ServiceNotFoundException.</param>
        public ServiceNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceNotFoundException" /> class.  Also contains an error
        ///     information message and InnerException.
        /// </summary>
        /// <param name="message">Associated error information for ServiceNotFoundException.</param>
        /// <param name="inner">Exception to set as InnerException.</param>
        public ServiceNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceNotFoundException" /> class. Also contains SerializationInfo
        ///     and StreamingContext.
        /// </summary>
        /// <param name="info">Information on how to serialize an ServiceNotFoundException.</param>
        /// <param name="context">Information on the streaming context for an ServiceNotFoundException.</param>
        protected ServiceNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        ///     Gets or sets the type of service that was not found.
        /// </summary>
        public Type ServiceType { get; set; }

        /// <summary>
        ///     Method to serialize the stored data for a ServiceNotFoundException.
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}