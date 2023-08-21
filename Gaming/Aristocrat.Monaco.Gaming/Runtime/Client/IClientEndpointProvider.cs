namespace Aristocrat.Monaco.Gaming.Runtime.Client
{
    /// <summary>
    ///     Provides a mechanism to manage the active communications channel
    /// </summary>
    /// <typeparam name="T">The client type</typeparam>
    public interface IClientEndpointProvider<T>
        where T : IClientEndpoint
    {
        /// <summary>
        ///     Gets the current client callback channel
        /// </summary>
        T Client { get; }

        /// <summary>
        ///     Adds or updates the current client callback
        /// </summary>
        /// <param name="client">The current client callback</param>
        void AddOrUpdate(T client);

        /// <summary>
        ///     Clears the current client callback
        /// </summary>
        void Clear();
    }
}