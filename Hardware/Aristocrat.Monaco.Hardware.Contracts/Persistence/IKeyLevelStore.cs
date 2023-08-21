namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System.Collections.Generic;

    /// <summary> Interface for key level store. </summary>
    public interface IKeyLevelStore
    {
        /// <summary> Adds or update key persistence level. </summary>
        /// <param name="key">   The key. </param>
        /// <param name="level"> The persistence level. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool AddOrUpdateLevel(string key, PersistenceLevel level);

        /// <summary> Attempts to get the value associated with the given key. </summary>
        /// <param name="key">   The key. </param>
        /// <param name="level"> [out] The value. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool TryGetLevel(string key, out PersistenceLevel level);

        /// <summary> Attempts to remove key level. </summary>
        /// <param name="key"> The key. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool TryRemoveLevel(string key);

        /// <summary> Attempts to remove key levels. </summary>
        /// <param name="keys"> The values. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool TryRemoveLevel(IEnumerable<string> keys);

        /// <summary> Gets (key,persistence) information. </summary>
        /// <returns> An IEnumerator&lt;KeyValuePair&lt;string,PersistenceLevel&gt;&gt; </returns>
        IEnumerable<KeyValuePair<string, PersistenceLevel>> LevelData();

        /// <summary> Enumerates level keys in this collection. </summary>
        /// <returns>
        /// An enumerator that allows foreach to be used to process level keys in this collection.
        /// </returns>
        IEnumerable<string> LevelKeys();
    }
}