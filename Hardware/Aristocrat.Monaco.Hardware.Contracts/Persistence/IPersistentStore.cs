namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    /// <summary> Interface for persistent store. </summary>
    public interface IPersistentStore : IKeyLevelStore, IKeyValueStore
    {
        /// <summary> Verifies the data integrity of the persistent store. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool Verify(bool full);
    }
}