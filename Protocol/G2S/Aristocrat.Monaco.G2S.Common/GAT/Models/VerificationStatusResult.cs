namespace Aristocrat.Monaco.G2S.Common.GAT.Models
{
    using System.Collections.Generic;

    /// <summary>
    ///     Verification status result
    /// </summary>
    public class VerificationStatusResult
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VerificationStatusResult" /> class.
        /// </summary>
        /// <param name="verificationCompleted">Parameter that defines verification process completed or not</param>
        /// <param name="componentVerificationResults">List of components for which the verification process completed</param>
        /// <param name="verificationStatus">List components that did not verify</param>
        public VerificationStatusResult(
            bool verificationCompleted,
            IList<ComponentVerificationResult> componentVerificationResults,
            VerificationStatus verificationStatus)
        {
            VerificationCompleted = verificationCompleted;
            ComponentVerificationResults = componentVerificationResults;
            VerificationStatus = verificationStatus;
        }

        /// <summary>
        ///     Gets a value indicating whether the verification process completed or not.
        /// </summary>
        public bool VerificationCompleted { get; }

        /// <summary>
        ///     Gets list of components for which the verification request completed
        /// </summary>
        public IList<ComponentVerificationResult> ComponentVerificationResults { get; }

        /// <summary>
        ///     Gets list components that did not verify
        /// </summary>
        public VerificationStatus VerificationStatus { get; }
    }
}