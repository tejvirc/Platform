namespace Aristocrat.G2S.Client
{
    using Devices;

    /// <summary>
    ///     Provides a mechanism to attach and get handlers for commands.
    /// </summary>
    public interface IHandlerConnector
    {
        /// <summary>
        ///     Gets a handler for the provided command.
        /// </summary>
        /// <param name="command">ClassCommand instance used to lookup the handler.</param>
        /// <returns>A command handler if found or null.</returns>
        ICommandHandler GetHandler(ClassCommand command);

        /// <summary>
        ///     Registers a handler.
        /// </summary>
        /// <param name="handler">An instance of an ICommandHandler</param>
        void RegisterHandler(ICommandHandler handler);

        /// <summary>
        ///     Determines if a handler has been added for the class in the provided command.
        /// </summary>
        /// <param name="command">ClassCommand instance used to search for a supported class.</param>
        /// <returns>true if found otherwise false.</returns>
        bool IsClassSupported(ClassCommand command);

        /// <summary>
        ///     Clears all registered handlers
        /// </summary>
        void Clear();
    }
}