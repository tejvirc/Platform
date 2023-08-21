namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System.Collections.Generic;

    /// <summary> Interface for key value store. </summary>
    public interface IKeyValueStore
    {
        /// <summary>
        ///     Adds a key/value pair if the key does not already exist,
        ///     or updates a key/value pair if the key already exists.
        /// </summary>
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="key">   The key. </param>
        /// <param name="value"> The value. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool AddOrUpdateValue<T>(string key, T value);

        /// <summary> Adds an or update. </summary>
        /// <param name="values"> The values. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool AddOrUpdateValue(IEnumerable<(string key, object value)> values);

        /// <summary> Attempts to get the value associated with the given key. </summary>
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="key">   The key. </param>
        /// <param name="value"> [out] The value. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool TryGetValue<T>(string key, out T value);

        /// <summary> Attempts to remove value. </summary>
        /// <param name="key"> The key. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool TryRemoveValue(string key);

        /// <summary> Attempts to remove value. </summary>
        /// <param name="keys"> The keys. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool TryRemoveValue(IEnumerable<string> keys);

        /// <summary> Gets (key,persistence) information. </summary>
        /// <returns> An IEnumerator&lt;KeyValuePair&lt;string,PersistenceLevel&gt;&gt; </returns>
        IEnumerable<KeyValuePair<string, object>> ValueData();

        /// <summary> Enumerates value keys in this collection. </summary>
        /// <returns>
        ///     An enumerator that allows foreach to be used to process value keys in this collection.
        /// </returns>
        IEnumerable<string> ValueKeys();
    }
}