namespace Aristocrat.Monaco.Mgam.Commands
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using Aristocrat.Mgam.Client.Messaging;

    /// <summary>
    ///     Wraps a <see cref="ServerResponseCode"/> in an exception.
    /// </summary>
    [Serializable]
    public class RegistrationException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RegistrationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="failureBehavior">Determines how the registration service should handle failed registration.</param>
        public RegistrationException(string message, RegistrationFailureBehavior failureBehavior)
            : base(message)
        {
            FailureBehavior = failureBehavior;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RegistrationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="failureBehavior">Determines how the registration service should handle failed registration.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public RegistrationException(string message, Exception innerException, RegistrationFailureBehavior failureBehavior)
            : base(message, innerException)
        {
            FailureBehavior = failureBehavior;
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

            info.AddValue("FailureBehavior", FailureBehavior);
        }

        /// <summary>
        ///     Gets a value that determines how the registration service should handle failed registration.
        /// </summary>
        public RegistrationFailureBehavior FailureBehavior { get; }
    }
}
