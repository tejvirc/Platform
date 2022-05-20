namespace Aristocrat.Monaco.Hardware.Contracts
{
    using Persistence;

    /// <summary>Interface for storage accessor.</summary>
    /// <typeparam name="TBlock">Type of the block.</typeparam>
    public interface IStorageAccessor<TBlock>
    {
        /// <summary>Attempts to initialize block.</summary>
        /// <param name="accessor">The accessor.</param>
        /// <param name="blockIndex">Zero-based index of the block.</param>
        /// <param name="block">[out] The block.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        bool TryAddBlock(IPersistentStorageAccessor accessor, int blockIndex, out TBlock block);

        /// <summary>Attempts to read block.</summary>
        /// <param name="accessor">The accessor.</param>
        /// <param name="blockIndex">Zero-based index of the block.</param>
        /// <param name="block">[out] The block.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        bool TryGetBlock(IPersistentStorageAccessor accessor, int blockIndex, out TBlock block);
    }
}
