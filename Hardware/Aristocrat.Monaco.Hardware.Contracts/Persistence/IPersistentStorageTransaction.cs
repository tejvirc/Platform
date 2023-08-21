namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System;

    /// <summary>
    ///     IPersistentStorageTransaction interface.
    /// </summary>
    public interface IPersistentStorageTransaction : IDisposable
    {
        /// <summary>Gets or sets the field at the first element of the array.</summary>
        /// <param name="blockFieldName">Block Field Name</param>
        /// <returns>An object</returns>
        object this[string blockFieldName] { get; set; }

        /// <summary>Gets or sets the field at the first element of the array in the named block.</summary>
        /// <param name="blockName">Block Name</param>
        /// <param name="blockFieldName">Block Field Name</param>
        /// <returns>An object</returns>
        object this[string blockName, string blockFieldName] { get; set; }

        /// <summary>
        ///     Return a field from the object saved in the persistent storage
        ///     array for this block.
        /// </summary>
        /// <param name="arrayIndex">Array Index</param>
        /// <param name="blockFieldName">Block Field Name</param>
        /// <returns>A field</returns>
        object this[int arrayIndex, string blockFieldName] { get; set; }

        /// <summary>
        ///     Return a field from the object saved in the persistent storage
        ///     array for the named block.
        /// </summary>
        /// <param name="blockName">Block Name</param>
        /// <param name="arrayIndex">Array Index</param>
        /// <param name="blockFieldName">Block Field Name</param>
        /// <returns>A field</returns>
        object this[string blockName, int arrayIndex, string blockFieldName] { get; set; }

        /// <summary>
        ///     Registers an event handler fired when the transaction is completed
        /// </summary>
        event EventHandler<TransactionEventArgs> OnCompleted;

        /// <summary>
        ///     Commit all field updates since transaction creation.
        /// </summary>
        void Commit();

        /// <summary>
        ///     Roll back all field updates since transaction creation.
        /// </summary>
        void Rollback();

        /// <summary>
        ///     Adds another block to the transaction that can be committed at the same time.
        /// </summary>
        /// <param name="block">The new block</param>
        void AddBlock(IPersistentStorageAccessor block);
    }
}