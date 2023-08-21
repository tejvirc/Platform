namespace Aristocrat.Monaco.Bingo.Common.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class RegistrationException : Exception
    {
        public RegistrationException(string message, RegistrationFailureReason reason)
            : base(message)
        {
            Reason = reason;
        }

        public RegistrationException(string message, Exception innerException, RegistrationFailureReason reason)
            : base(message, innerException)
        {
            Reason = reason;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            base.GetObjectData(info, context);
            info.AddValue(nameof(Reason), Reason);
        }

        public RegistrationFailureReason Reason { get; }
    }
}