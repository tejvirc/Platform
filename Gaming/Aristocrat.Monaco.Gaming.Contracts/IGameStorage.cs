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
        /// <param name="gameId"> The game identifier. </param>
        /// <param name="betAmount"> The bet amount (in credits) for which the associated meter is to be obtained.</param>
        /// <param name="name">The value name</param>
        /// <returns>The value associated with the provided name</returns>
        T GetValue<T>(int gameId, long betAmount, string name);

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
    }
}
