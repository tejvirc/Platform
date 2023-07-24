namespace Aristocrat.G2S.Client.Communications
{
    /// <summary>
    ///     Provides a mechanism for receiving push-based notifications for commands and exceptions
    /// </summary>
    public interface IMessageReceiver
    {
        /// <summary>
        ///     Notifies the observer that the a ClassCommand has been received.
        /// </summary>
        /// <param name="command">ClassCommand instance that was received.</param>
        void Receive(ClassCommand command);
    }
}