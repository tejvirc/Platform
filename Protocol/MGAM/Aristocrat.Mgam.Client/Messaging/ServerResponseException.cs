namespace Aristocrat.Mgam.Client.Messaging
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    ///     Wraps a <see cref="ServerResponseCode"/> in an exception.
    /// </summary>
    [Serializable]
    public class ServerResponseException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ServerResponseException"/> class.
        /// </summary>
        /// <param name="code">Response code sent from the server.</param>
        public ServerResponseException(ServerResponseCode code)
            : base($"Response code {code} was returned from the server")
        {
            ResponseCode = code;
        }

        /// <summary>
        ///     When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue("ResponseCode", ResponseCode);
        }

        /// <summary>
        ///     Gets the <see cref="ServerResponseCode"/> returned from the server.
        /// </summary>
        public ServerResponseCode ResponseCode { get; }
    }
}
