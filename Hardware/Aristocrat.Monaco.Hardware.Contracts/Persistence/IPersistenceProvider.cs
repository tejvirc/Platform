namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System.Threading.Tasks;

    /// <summary> Interface for persistence provider. </summary>
    public interface IPersistenceProvider
    {
        /// <summary> Gets a block. </summary>
        /// <param name="key"> The key. </param>
        /// <returns> The block. </returns>
        IPersistentBlock GetBlock(string key);

        /// <summary> Gets or Creates a block. </summary>
        /// <param name="key">   The key. </param>
        /// <param name="level"> The level. </param>
        /// <returns> The new block. </returns>
        IPersistentBlock GetOrCreateBlock(string key, PersistenceLevel level);

        /// <summary>
        /// Creates a new scoped transaction if one already doesn't exist.
        /// If one exists, returns that existing scoped transaction.
        /// </summary>
        /// <returns> An IScopedTransaction. </returns>
        IScopedTransaction ScopedTransaction();

        /// <summary>
        /// Verifies data integrity for all the configured persistent stores.
        /// </summary>
        /// <param name="full"> Tells whether to run full or quick verification. </param>
        /// <returns> A task object. </returns>
        Task Verify(bool full);
    }
}