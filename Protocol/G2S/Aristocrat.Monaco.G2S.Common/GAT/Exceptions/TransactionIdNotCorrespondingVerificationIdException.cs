namespace Aristocrat.Monaco.G2S.Common.GAT.Exceptions
{
    using System;

    /// <summary>
    ///     Transaction id not corresponding verification id exception
    /// </summary>
    [Serializable]
    public class TransactionIdNotCorrespondingVerificationIdException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionIdNotCorrespondingVerificationIdException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public TransactionIdNotCorrespondingVerificationIdException(string message)
            : base(message)
        {
        }
    }
}