namespace Aristocrat.Monaco.Common
{
    /// <summary>
    ///     Provides a mechanism to attach and get a handler
    /// </summary>
    public interface IHandlerConnector<T>
    {
        /// <summary>
        ///     Gets the registered handler
        /// </summary>
        /// <returns>A handler if found or null.</returns>
        T Handler { get; }

        /// <summary>
        ///     Registers a handler.
        /// </summary>
        /// <param name="handler">An instance of type T</param>
        /// <param name="name">The name of the entity registers the handler</param>
        /// <returns>Whether or not the handler was registered</returns>
        bool Register(T handler, string name);

        /// <summary>
        ///     Clears the registered handler
        /// </summary>
        /// <param name="name">The name of the entity that tries to clear the handler</param>
        /// <returns>Whether or not the handler was cleared</returns>
        bool Clear(string name);
    }
}