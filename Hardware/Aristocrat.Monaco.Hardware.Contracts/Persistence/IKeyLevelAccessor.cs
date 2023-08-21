namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    /// <summary> Interface for key level accessor. </summary>
    public interface IKeyLevelAccessor
    {
        /// <summary> Gets persistence level of the given key. </summary>
        /// <param name="key">   The key. </param>
        /// <param name="level"> [out] The level. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool GetPersistenceLevel(string key, out PersistenceLevel level);

        /// <summary> Sets persistence level. </summary>
        /// <param name="key"> The key. </param>
        /// <param name="level"> The persistence level. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool SetPersistenceLevel(string key, PersistenceLevel level);
    }
}