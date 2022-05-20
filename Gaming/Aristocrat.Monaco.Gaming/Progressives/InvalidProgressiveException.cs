namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class InvalidProgressiveException : Exception
    {
        public InvalidProgressiveException()
        {
        }

        public InvalidProgressiveException(string message) : base(message)
        {
        }

        public InvalidProgressiveException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidProgressiveException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}