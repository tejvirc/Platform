namespace Aristocrat.Monaco.G2S.Common.GAT.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Unknown component exception
    /// </summary>
    [Serializable]
    public class UnknownComponentException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UnknownComponentException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public UnknownComponentException(string message)
            : base(message)
        {
        }
    }
}