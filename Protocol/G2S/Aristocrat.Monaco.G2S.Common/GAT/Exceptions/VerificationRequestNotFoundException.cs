namespace Aristocrat.Monaco.G2S.Common.GAT.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Verification request not found exception
    /// </summary>
    [Serializable]
    public class VerificationRequestNotFoundException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VerificationRequestNotFoundException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public VerificationRequestNotFoundException(string message)
            : base(message)
        {
        }
    }
}