namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using System.Collections.Generic;
    using Linked;

    /// <summary>
    ///     Provides an interface for reporting, clearing, and viewing progressive errors 
    /// </summary>
    public interface IProgressiveErrorProvider
    {
        /// <summary>
        ///     Reports a progressive disconnected error for a given progressive group id
        /// </summary>
        /// <param name="levels">The progressive levels that are disconnected</param>
        void ReportProgressiveDisconnectedError(IEnumerable<IViewableLinkedProgressiveLevel> levels);

        /// <summary>
        ///     Clears a progressive disconnected error for a given progressive group id
        /// </summary>
        /// <param name="levels">The progressive levels that are now reconnected</param>
        void ClearProgressiveDisconnectedError(IEnumerable<IViewableLinkedProgressiveLevel> levels);

        /// <summary>
        ///     Provides a view for active progressive disconnect errors
        /// </summary>
        /// <returns></returns>
        IEnumerable<int> ViewProgressiveDisconnectedErrors();

        /// <summary>
        ///     Reports a progressive claim timeout error
        /// </summary>
        /// <param name="levels">The levels that had a claim timeout</param>
        void ReportProgressiveClaimTimeoutError(IEnumerable<IViewableLinkedProgressiveLevel> levels);

        /// <summary>
        ///     Clears the claim timeout error from the provided level
        /// </summary>
        /// <param name="levels">The levels that had the claim timeout cleared</param>
        void ClearProgressiveClaimError(IEnumerable<IViewableLinkedProgressiveLevel> levels);

        /// <summary>
        ///     Reports the minimum threshold error
        /// </summary>
        /// <param name="levels">The levels who had a minimum threshold error</param>
        void ReportMinimumThresholdError(IEnumerable<IViewableProgressiveLevel> levels);

        /// <summary>
        ///     Clears the minimum threshold error from the provided levels
        /// </summary>
        /// <param name="levels">The levels who had the minimum threshold error cleared</param>
        void ClearMinimumThresholdError(IEnumerable<IViewableProgressiveLevel> levels);

        /// <summary>
        ///     Reports a progressive update timeout error for a given progressive group id
        /// </summary>
        /// <param name="levels">The progressive levels that have timed out</param>
        void ReportProgressiveUpdateTimeoutError(IEnumerable<IViewableLinkedProgressiveLevel> levels);

        /// <summary>
        ///     Clears a progressive disconnected error for a given progressive group id
        /// </summary>
        /// <param name="levels">The progressive levels that have cleared the update timeout</param>
        void ClearProgressiveUpdateError(IEnumerable<IViewableLinkedProgressiveLevel> levels);

        /// <summary>
        ///     Provides a view for active progressive disconnect errors
        /// </summary>
        /// <returns></returns>
        IEnumerable<int> ViewProgressiveUpdateTimeoutErrors();

        /// <summary>
        ///     Checks the provides levels for any errors
        /// </summary>
        /// <param name="levels">The levels to check for errors</param>
        void CheckProgressiveLevelErrors(IEnumerable<IViewableProgressiveLevel> levels);
    }
}