namespace Aristocrat.Monaco.G2S.Common.GAT.Models
{
    using System.Collections.Generic;

    /// <summary>
    ///     Verification status
    /// </summary>
    public class VerificationStatus
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VerificationStatus" /> class.
        /// </summary>
        /// <param name="verificationId">Verification identifier</param>
        /// <param name="transactionId">Transaction identifier</param>
        /// <param name="componentStatuses">List processed components</param>
        public VerificationStatus(
            long verificationId,
            long transactionId,
            IEnumerable<ComponentStatus> componentStatuses)
        {
            VerificationId = verificationId;
            TransactionId = transactionId;
            ComponentStatuses = componentStatuses;
        }

        /// <summary>
        ///     Gets verification identifier
        /// </summary>
        public long VerificationId { get; }

        /// <summary>
        ///     Gets Transaction identifier
        /// </summary>
        public long TransactionId { get; }

        /// <summary>
        ///     Gets list processed components
        /// </summary>
        public IEnumerable<ComponentStatus> ComponentStatuses { get; }
    }
}