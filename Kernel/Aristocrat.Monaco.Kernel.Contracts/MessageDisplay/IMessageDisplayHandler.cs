namespace Aristocrat.Monaco.Kernel
{
    /// <summary>
    ///     Definition of the IMessageDisplayHandler interface.
    /// </summary>
    /// <remarks>
    ///     Calls to methods in this interface are made from an object that provides services accessible via the
    ///     <see cref="IMessageDisplay" />
    ///     interface. A class that implements IMessageDisplayHandler should register itself with the IMessageDisplay service.
    ///     It will then
    ///     receive calls with messages from that service. The implementation of a message display handler component determines
    ///     how messages
    ///     are presented to the outside world.
    /// </remarks>
    /// <seealso cref="IMessageDisplay" />
    /// <seealso cref="DisplayableMessage" />
    public interface IMessageDisplayHandler
    {
        /// <summary>
        ///     Receives a message to display from the message display service.
        /// </summary>
        /// <param name="displayableMessage">The message to display</param>
        void DisplayMessage(DisplayableMessage displayableMessage);

        /// <summary>
        ///     Removes the message from the display as instructed by the message display service.
        /// </summary>
        /// <param name="displayableMessage">A reference to the message to remove</param>
        void RemoveMessage(DisplayableMessage displayableMessage);

        /// <summary>
        ///     Displays a status message sent from the message display service.
        /// </summary>
        /// <param name="message">The status message to display</param>
        void DisplayStatus(string message);

        /// <summary>
        ///     Informs the display handler to remove all messages it is displaying.
        /// </summary>
        /// <remarks>
        ///     Typically, this method is called by the message display service when a new message display handler supersedes
        ///     the currently active handler.
        /// </remarks>
        void ClearMessages();
    }
}