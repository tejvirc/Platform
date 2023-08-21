namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System;

    /// <summary>
    ///     Interface for persistent block.
    /// </summary>
    public interface IPersistentBlock : IKeyValueAccessor, IDisposable
    {
        /// <summary>
        ///     Gets the name of the block.
        /// </summary>
        /// <value>The name of the block.</value>
        string BlockName { get; }

        /// <summary>
        ///     Gets the level.
        /// </summary>
        /// <value>The level.</value>
        PersistenceLevel PersistenceLevel { get; }

        /// <summary>
        ///     Get a persistent transaction.
        /// </summary>
        /// <returns>A reference to ITransaction implementation.</returns>
        IPersistentTransaction Transaction();
    }
}