namespace Aristocrat.Sas.Client
{
    /// <summary>
    ///     A queue for holding messages needing to be sent SAS on the next general poll
    /// </summary>
    public interface ISasMessageQueue
    {
        /// <summary>
        ///     Gets whether or not the queue is currently empty
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        ///     This will enqueue a message into the message queue
        /// </summary>
        /// <param name="message">The message to enqueue</param>
        void QueueMessage(ISasMessage message);

        /// <summary>
        ///     This will get a message from the message queue or return an empty message
        /// </summary>
        /// <returns>The message or empty message</returns>
        ISasMessage GetNextMessage();

        /// <summary>
        ///     Used to acknowledge the last message that was read from the Queue, causing the queued messages to be cleared
        /// </summary>
        void MessageAcknowledged();

        /// <summary>
        ///     Used to clear any pending message reads leaving the read message in the queue
        /// </summary>
        void ClearPendingMessage();
    }
}