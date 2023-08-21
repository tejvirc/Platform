namespace Aristocrat.Monaco.Sas.Base
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     SasHostDiscoveryException
    /// </summary>
    [Serializable]
    internal class SasHostDiscoveryException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SasHostDiscoveryException" /> class.
        /// </summary>
        /// <param name="message">the message</param>
        public SasHostDiscoveryException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SasHostDiscoveryException" /> class.
        /// </summary>
        /// <param name="info">the info</param>
        /// <param name="context">the context</param>
        protected SasHostDiscoveryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}