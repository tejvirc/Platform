namespace Aristocrat.Monaco.Bingo.Common.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message, ConfigurationFailureReason reason)
            : base(message)
        {
            Reason = reason;
        }

        public ConfigurationException(string message, Exception innerException, ConfigurationFailureReason reason)
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

        public ConfigurationFailureReason Reason { get; }
    }
}