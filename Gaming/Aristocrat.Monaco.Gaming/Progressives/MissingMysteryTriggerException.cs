namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This Exception is thrown when a Mystery Progressive Trigger value cannot be loaded from the database. This should never happen. The Mystery Trigger value is the amount a Mystery progressive level should pay out when the player wins the level.
    /// </summary>
    [Serializable]
    internal class MissingMysteryTriggerException : Exception
    {

        public MissingMysteryTriggerException()
        {
        }

        public MissingMysteryTriggerException(string message) : base(message)
        {
        }

        public MissingMysteryTriggerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MissingMysteryTriggerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
