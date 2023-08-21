namespace Aristocrat.Monaco.Application.Contracts
{
    /// <summary>
    ///     An interface for an object that governs whether or not clearing of persisted data should be allowed.
    /// </summary>
    public interface IPersistenceClearArbiter
    {
        /// <summary>Gets a value indicating whether or not partial persistence clear is allowed</summary>
        bool PartialClearAllowed { get; }

        /// <summary>Gets a value indicating whether or not full persistence clear is allowed</summary>
        bool FullClearAllowed { get; }

        /// <summary>Gets human-readble text describing why partial clear is not allowed</summary>
        string[] PartialClearDeniedReasons { get; }

        /// <summary>Gets human-readble text describing why full clear is not allowed</summary>
        string[] FullClearDeniedReasons { get; }
    }
}