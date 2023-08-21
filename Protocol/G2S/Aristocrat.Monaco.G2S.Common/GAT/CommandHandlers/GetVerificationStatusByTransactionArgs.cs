namespace Aristocrat.Monaco.G2S.Common.GAT.CommandHandlers
{
    using System;

    /// <summary>
    ///     Get verification status by transaction arguments
    /// </summary>
    public class GetVerificationStatusByTransactionArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GetVerificationStatusByTransactionArgs" /> class.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="verificationId">The verification identifier.</param>
        public GetVerificationStatusByTransactionArgs(long transactionId, long verificationId)
        {
            if (verificationId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(verificationId), @"Must be more than zero");
            }

            if (transactionId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(transactionId), @"Must be more than zero");
            }

            TransactionId = transactionId;
            VerificationId = verificationId;
        }

        /// <summary>
        ///     Gets transaction identifier
        /// </summary>
        public long TransactionId { get; }

        /// <summary>
        ///     Gets verification identifier
        /// </summary>
        public long VerificationId { get; }
    }
}