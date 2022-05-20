namespace Aristocrat.Monaco.Application.Contracts
{
    using System;

    /// <summary>
    ///     An interface for a rule that governs whether or not clearing of persisted data should be allowed.
    /// </summary>
    public interface IPersistenceClearRule
    {
        /// <summary>Gets a value indicating whether or not partial persistence clear is allowed</summary>
        bool PartialClearAllowed { get; }

        /// <summary>Gets a value indicating whether or not full persistence clear is allowed</summary>
        bool FullClearAllowed { get; }

        /// <summary>Gets human-readble text describing why persistence clear is not allowed</summary>
        string ClearDeniedReason { get; }

        /// <summary>The event to notify a that partial or full clear allowance has changed.</summary>
        event EventHandler RuleChangedEvent;
    }
}