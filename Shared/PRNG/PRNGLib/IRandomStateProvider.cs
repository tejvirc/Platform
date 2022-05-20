namespace PRNGLib
{
    /// <summary>
    ///     Provides an optional mechanism to restore and save the state of the RNG
    /// </summary>
    public interface IRandomStateProvider
    {
        /// <summary>
        ///     Gets the current key for the RNG
        /// </summary>
        /// <returns>The key if any</returns>
        byte[] ReadKey();

        /// <summary>
        ///     Writes the current RNG key
        /// </summary>
        /// <param name="key">The current key</param>
        void WriteKey(byte[] key);
    }
}