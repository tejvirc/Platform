namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System;

    /// <summary> Interface for persistent transaction. </summary>
    public interface IPersistentTransaction : IKeyValueAccessor, IDisposable
    {
        /// <summary>
        /// Event Handler for Completed event.
        /// </summary>
        event EventHandler<TransactionEventArgs> Completed;

        /// <summary>
        /// Commits updates done in the transaction.
        /// </summary>
        /// <returns>True if it succeeds, false if it fails.</returns>
        bool Commit();
    }
}