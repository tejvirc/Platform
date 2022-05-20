namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System;

    /// <summary>
    ///     Definition of the IPersistentStorageManager interface.
    /// </summary>
    public interface IPersistentStorageManager
    {
        /// <summary>
        ///     Registers an event handler for clearing persistent storage.
        /// </summary>
        event EventHandler<StorageEventArgs> StorageClearingEventHandler;

        /// <summary>
        ///     Registers an event handler for persistent storage cleared.
        /// </summary>
        event EventHandler<StorageEventArgs> StorageClearedEventHandler;

        /// <summary>
        ///     Destroys all blocks and sets every byte of the storage media to a zero value.
        /// </summary>
        /// <param name="level">The persistent storage level to clear the data from.</param>
        void Clear(PersistenceLevel level);

        /// <summary>
        ///     Returns whether or not data integrity is intact for the entire storage media.
        /// </summary>
        /// <param name="full">If true, does a full check, otherwise a quick check.</param>
        /// <returns>Whether or not data integrity is intact for the entire storage media.</returns>
        bool VerifyIntegrity(bool full);

        /// <summary>
        ///     Defragment all persistent storage.
        /// </summary>
        void Defragment();

        /// <summary>
        ///     Creates a uniquely named block of memory for the desired size.
        /// </summary>
        /// <param name="level">The persistent storage level to store the data at.</param>
        /// <param name="name">A unique name by which to reference the block.</param>
        /// <param name="arraySize">The number of elements in this block.  (Usually 1 -- one element with multiple fields.)</param>
        /// <returns>A reference to the created block.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when size parameter is negative.
        /// </exception>
        /// <exception cref="DuplicateBlockException">
        ///     Thrown when name parameter is identical to that of an existing block.
        ///     Thrown when there is not enough memory available to create the block.
        /// </exception>
        IPersistentStorageAccessor CreateBlock(PersistenceLevel level, string name, int arraySize);

        /// <summary>
        ///     Creates a uniquely named dynamic block of memory for the desired size.
        /// </summary>
        /// <param name="level">The persistent storage level to store the data at.</param>
        /// <param name="name">A unique name by which to reference the block.</param>
        /// <param name="arraySize">The number of elements in this block.  (Usually 1 -- one element with multiple fields.)</param>
        /// <param name="format">The desired format of the new dynamic block.</param>
        /// <returns>A reference to the created block.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when size parameter is negative.
        /// </exception>
        /// <exception cref="DuplicateBlockException">
        ///     Thrown when name parameter is identical to that of an existing block.
        ///     Thrown when there is not enough memory available to create the block.
        /// </exception>
        IPersistentStorageAccessor CreateDynamicBlock(
            PersistenceLevel level,
            string name,
            int arraySize,
            BlockFormat format);

        /// <summary>
        ///     Returns whether or not a block with the given name exists.
        /// </summary>
        /// <param name="name">The unique name of the block.</param>
        /// <returns>Whether or not a block with the given name exists.</returns>
        bool BlockExists(string name);

        /// <summary>
        ///     Returns whether or not a block with the given name exists.
        /// </summary>
        /// <param name="name">The unique name of the block.</param>
        /// <returns>A reference to the block of the given name.</returns>
        /// <exception cref="BlockNotFoundException">
        ///     Thrown when a block with the name parameter does not exist.
        /// </exception>
        IPersistentStorageAccessor GetBlock(string name);

        /// <summary>
        ///     Changes the size of the specified block to the specified size.
        /// </summary>
        /// <param name="name">The unique name of the block.</param>
        /// <param name="size">The number of elements in the array.</param>
        /// <exception cref="BlockNotFoundException">
        ///     Thrown when a block with the name parameter does not exist.
        /// </exception>
        void ResizeBlock(string name, int size);

        /// <summary>
        ///     Updates the <see cref="PersistenceLevel"/> to the level specified
        /// </summary>
        /// <param name="name">The unique name of the block</param>
        /// <param name="level">The new level</param>
        void UpdatePersistenceLevel(string name, PersistenceLevel level);

        /// <summary>
        ///     Starts a transaction that can be used across one or more blocks
        /// </summary>
        /// <returns>An IScopedTransaction</returns>
        IScopedTransaction ScopedTransaction();
    }
}