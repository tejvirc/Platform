namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System.Collections.Generic;

    /// <summary>
    ///     Definition of the IPersistentStorageAccessor interface.
    /// </summary>
    public interface IPersistentStorageAccessor
    {
        /// <summary>
        ///     Gets the name of the block array
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the count of the number of items in the block array
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the BlockFormat for the current block.
        /// </summary>
        BlockFormat Format { get; }

        /// <summary>
        ///     Gets the level of the persistence block.
        /// </summary>
        PersistenceLevel Level { get; }

        /// <summary>
        ///     Gets the format version level of the persistence block.
        /// </summary>
        int Version { get; }

        /// <summary>Gets or sets the field at the first element of the array.</summary>
        /// <param name="blockFieldName">Block Field Name</param>
        /// <returns>An object</returns>
        object this[string blockFieldName] { get; set; }

        /// <summary>
        ///     Return a field from the object saved in the persistent storage
        ///     array for this block.
        /// </summary>
        /// <param name="arrayIndex">Array Index</param>
        /// <param name="blockFieldName">Block Field Name</param>
        /// <returns>A field</returns>
        object this[int arrayIndex, string blockFieldName] { get; set; }

        /// <summary>
        ///     Gets all key/value pairs for the block
        /// </summary>
        /// <returns>a Dictionary where the key is the index and value represents the field and it's value</returns>
        IDictionary<int, Dictionary<string, object>> GetAll();

        /// <summary>Called to lock the block to the thread.</summary>
        /// <param name="waitForLock">Whether to block until the lock is attained.</param>
        /// <returns>Whether the lock was successful.</returns>
        bool StartUpdate(bool waitForLock);

        /// <summary>Called to unlock the block and write the changes to persistent storage.</summary>
        void Commit();

        /// <summary>
        ///     RollBack to old values
        /// </summary>
        void Rollback();

        /// <summary>
        ///     Start a transaction.
        /// </summary>
        /// <returns>An IPersistentStorageTransaction.</returns>
        IPersistentStorageTransaction StartTransaction();
    }
}