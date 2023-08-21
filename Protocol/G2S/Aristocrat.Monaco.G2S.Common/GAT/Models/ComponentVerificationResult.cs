namespace Aristocrat.Monaco.G2S.Common.GAT.Models
{
    using Storage;

    /// <summary>
    ///     Component verification result
    /// </summary>
    public class ComponentVerificationResult
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ComponentVerificationResult" /> class.
        /// </summary>
        /// <param name="componentId">Component identifier</param>
        /// <param name="state">Component verification state</param>
        /// <param name="gatExec">Designates the procedure that should be used to process the results</param>
        /// <param name="verifyResult">Result of the verification request</param>
        public ComponentVerificationResult(
            string componentId,
            ComponentVerificationState state,
            string gatExec,
            string verifyResult)
        {
            ComponentId = componentId;
            State = state;
            GatExec = gatExec;
            VerifyResult = verifyResult;
        }

        /// <summary>
        ///     Gets component identifier
        /// </summary>
        public string ComponentId { get; }

        /// <summary>
        ///     Gets component verification state
        /// </summary>
        public ComponentVerificationState State { get; }

        /// <summary>
        ///     Gets designates the procedure that should be used to process the results
        /// </summary>
        public string GatExec { get; }

        /// <summary>
        ///     Gets result of the verification request
        /// </summary>
        public string VerifyResult { get; }
    }
}