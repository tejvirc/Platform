namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System.Collections.Generic;

    /// <summary>
    ///     Provides a mechanism to store game specific data
    /// </summary>
    public interface IGameStorage
    {
        /// <summary>
        ///     Gets the value stored for the specified game combo (id and denom)
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="name">The block name</param>
        /// <returns>The value associated with the provided name</returns>
        T GetValue<T>(string name);

        /// <summary>
        ///     Gets the value stored for the specified game combo (id and denom)
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="name">The block name</param>
        /// <param name="keyName">The extra key identifier</param>
        /// <returns>The value associated with the provided name</returns>
        T GetValue<T>(string name, string keyName);

        /// <summary>
        ///     Gets the value stored for the specified game combo (id and denom)
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="gameId"> The game identifier. </param>
        /// <param name="betAmount"> The bet amount (in credits) for which the associated meter is to be obtained.</param>
        /// <param name="name">The value name</param>
        /// <returns>The value associated with the provided name</returns>
        T GetValue<T>(int gameId, long betAmount, string name);


        /// <summary>
        ///     Gets the value stored for the specified game combo (id, denom) and additional keyName identifier
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="gameId"> The game identifier. </param>
        /// <param name="betAmount"> The bet amount (in credits) for which the associated meter is to be obtained.</param>
        /// <param name="name">The value name</param>
        /// <param name="keyName">The extra key identifier</param>
        /// <returns>The value associated with the provided name</returns>
        T GetValue<T>(int gameId, long betAmount, string name, string keyName);

        /// <summary>
        ///     Gets the values stored for the specified game combo (id and denom)
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="gameId"> The game identifier. </param>
        /// <param name="name">The value name</param>
        /// <returns>The values associated with the provided name</returns>
        IEnumerable<T> GetValues<T>(int gameId, string name);

        /// <summary>
        ///     Gets the values stored for the specified game combo (id and denom)
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="gameId"> The game identifier. </param>
        /// <param name="betAmount"> The bet amount (in credits) for which the associated meter is to be obtained.</param>
        /// <param name="name">The value name</param>
        /// <returns>The values associated with the provided name</returns>
        IEnumerable<T> GetValues<T>(int gameId, long betAmount, string name);

        /// <summary>
        ///     Gets the values stored for the specified game combo (id, denom) and additional keyName identifier
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="gameId"> The game identifier. </param>
        /// <param name="betAmount"> The bet amount (in credits) for which the associated meter is to be obtained.</param>
        /// <param name="name">The value name</param>
        /// <param name="keyName">The extra key identifier</param>
        /// <returns>The values associated with the provided name</returns>
        IEnumerable<T> GetValues<T>(int gameId, long betAmount, string name, string keyName);

        /// <summary>
        ///     Attempts to get the values stored for the specified game combo (id and denom)
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="gameId"> The game identifier. </param>
        /// <param name="name">The value name</param>
        /// <param name="values"></param>
        /// <returns>true if successful</returns>
        bool TryGetValues<T>(int gameId, string name, out IEnumerable<T> values);

        /// <summary>
        ///     Attempts to get the values stored for the specified game combo (id and denom)
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="gameId"> The game identifier. </param>
        /// <param name="betAmount"> The bet amount (in credits) for which the associated meter is to be obtained.</param>
        /// <param name="name">The value name</param>
        /// <param name="values"></param>
        /// <returns>true if successful</returns>
        bool TryGetValues<T>(int gameId, long betAmount, string name, out IEnumerable<T> values);

        /// <summary>
        ///     Attempts to get the values stored for the specified game combo (id, denom) and additional keyName identifier
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="gameId"> The game identifier. </param>
        /// <param name="betAmount"> The bet amount (in credits) for which the associated meter is to be obtained.</param>
        /// <param name="name">The value name</param>
        /// <param name="keyName">The extra key identifier</param>
        /// <param name="values"></param>
        /// <returns>true if successful</returns>
        bool TryGetValues<T>(int gameId, long betAmount, string name, string keyName, out IEnumerable<T> values);


        /// <summary>
        ///     Sets the value stored for the specified game combo (id and denom)
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="name">The value name</param>
        /// <param name="value">The value to save</param>
        void SetValue<T>(string name, T value);

        /// <summary>
        ///     Sets the value stored for the specified game combo (id and denom)
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="name">The value name</param>
        /// <param name="value">The value to save</param>
        /// <param name="keyName">The extra key identifier</param>
        void SetValue<T>(string name, string keyName, T value);

        /// <summary>
        ///     Sets the value stored for the specified game combo (id and denom)
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="gameId"> The game identifier. </param>
        /// <param name="name">The value name</param>
        /// <param name="value">The value to save</param>
        void SetValue<T>(int gameId, string name, T value);

        /// <summary>
        ///     Sets the value stored for the specified game combo (id and denom)
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="gameId"> The game identifier. </param>
        /// <param name="betAmount"> The bet amount (in credits) for which the associated meter is to be obtained.</param>
        /// <param name="name">The value name</param>
        /// <param name="value">The value to save</param>
        void SetValue<T>(int gameId, long betAmount, string name, T value);

        /// <summary>
        ///     Sets the value stored for the specified game combo (id, denom) and additional keyName identifier
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="gameId"> The game identifier. </param>
        /// <param name="betAmount"> The bet amount (in credits) for which the associated meter is to be obtained.</param>
        /// <param name="name">The value name</param>
        /// <param name="keyName">The extra key identifier</param>
        /// <param name="value">The value to save</param>
        void SetValue<T>(int gameId, long betAmount, string name, string keyName, T value);


        /// <summary>
        ///    Clears all values having a keyName given the particular storageName
        /// </summary>
        /// <param name="gameId"> The game identifier. </param>
        /// <param name="betAmount"> The bet amount (in credits) for which the associated meter is to be obtained.</param>
        /// <param name="name">The value name</param>
        void ClearAllValuesWithKeyName(int gameId, long betAmount, string name);

        /// <summary>
        ///    Clears all values having a keyName given the particular storageName
        /// </summary>
        /// <param name="name">The storageName </param>
        void ClearAllValuesWithKeyName(string name);

        /// <summary>
        ///     Returns the keys which are available for the given combination
        /// </summary>
        /// <param name="gameId"> The game identifier. </param>
        /// <param name="betAmount"> The bet amount (in credits) for which the associated meter is to be obtained.</param>
        /// <param name="name">The value name</param>
        /// <returns>Returns a collection of stored key values</returns>
        Dictionary<string, string> GetKeyNameAndValues(int gameId, long betAmount, string name);

        /// <summary>
        ///     Returns the keys which are available for the given combination
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns a collection of stored key values</returns>
        Dictionary<string, string> GetKeyNameAndValues(string name);

    }
}
