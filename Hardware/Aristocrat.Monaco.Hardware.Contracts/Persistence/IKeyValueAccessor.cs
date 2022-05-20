namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    /// <summary> Interface for key value accessor. </summary>
    public interface IKeyValueAccessor
    {
        /// <summary> Gets key value where key is derived from typeof T.</summary>
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="value"> [out] The value. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool GetValue<T>(out T value);

        /// <summary> Gets a key value.</summary>
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="key"> The key. </param>
        /// <param name="value"> [out] The value. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool GetValue<T>(string key, out T value);

        /// <summary> Gets an indexed key value where key is derived from typeof T. </summary>
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="index"> Zero-based index of the. </param>
        /// <param name="value"> [out] The value. </param>
        /// <returns> True if value exists, false otherwise. </returns>
        bool GetValue<T>(int index, out T value);

        /// <summary> Gets indexed value. </summary>
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="key"> The key. </param>
        /// <param name="index"> Zero-based index of the. </param>
        /// <param name="value"> [out] The value. </param>
        /// <returns> True if value exists, false otherwise. </returns>
        bool GetValue<T>(string key, int index, out T value);

        /// <summary> Gets a value if it exists or creates a new value where key is derived from typeof T. </summary>
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <returns> The or create. </returns>
        T GetOrCreateValue<T>() where T : new();

        /// <summary> Gets a value if it exists or creates a new value. </summary>
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="key"> The key. </param>
        /// <returns> The or create. </returns>
        T GetOrCreateValue<T>(string key) where T : new();

        /// <summary> Sets a key value in the persistent store where key is derived from typeof T.</summary>
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="value"> The value. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool SetValue<T>(T value);

        /// <summary> Sets a key value in the persistent store. </summary>
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="key"> The key. </param>
        /// <param name="value"> The value. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool SetValue<T>(string key, T value);

        /// <summary> Sets an indexed key value where key is derived from typeof T. </summary>
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="index"> Zero-based index of the. </param>
        /// <param name="value"> The value. </param>
        /// <returns> True if value is successfully set, false otherwise. </returns>
        bool SetValue<T>(int index, T value);

        /// <summary> Sets an indexed key value in the persistent store. </summary>
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="key"> The key. </param>
        /// <param name="index"> Zero-based index of the. </param>
        /// <param name="value"> The value. </param>
        /// <returns> True if value is successfully set, false otherwise. </returns>
        bool SetValue<T>(string key, int index, T value);
    }
}