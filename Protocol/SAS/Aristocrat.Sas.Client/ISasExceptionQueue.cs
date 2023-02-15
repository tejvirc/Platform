namespace Aristocrat.Sas.Client
{
    public interface ISasExceptionQueue : ISasExceptionAcknowledgeHandler
    {
        /// <summary>
        ///     Gets or sets a value indicating whether to notify when the exception queue is empty or not
        /// </summary>
        public bool NotifyWhenExceptionQueueIsEmpty { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the exception queue is empty or not
        /// </summary>
        public bool ExceptionQueueIsEmpty { get; }

        /// <summary>
        ///     Gets the client number for the exception queue
        /// </summary>
        byte ClientNumber { get; }

        /// <summary>
        ///     Adds an event to the exception queue
        /// </summary>
        /// <param name="exception">The exception to add</param>
        void QueueException(ISasExceptionCollection exception);

        /// <summary>
        ///     Gets the next exception in priority order
        /// </summary>
        /// <returns>The exception</returns>
        ISasExceptionCollection GetNextException();

        /// <summary>
        ///     Gets the next exception without altering it
        /// </summary>
        /// <returns>The exception</returns>
        ISasExceptionCollection Peek();

        /// <summary>
        ///     Adds a priority exception
        /// </summary>
        /// <param name="exception">The exception to add</param>
        void QueuePriorityException(GeneralExceptionCode exception);

        /// <summary>
        ///     Converts a Real Time Event Reporting format exception into
        ///     the one byte version of the exception.
        ///     If the exception is already a one byte version it just
        ///     returns the exception.
        /// </summary>
        /// <param name="exception">The exception to be converted</param>
        /// <returns>A one byte version of the exception</returns>
        GeneralExceptionCode ConvertRealTimeExceptionToNormal(ISasExceptionCollection exception);

        /// <summary>
        ///     Used for acknowledging the last read exception and removing it from the queue
        /// </summary>
        void ExceptionAcknowledged();

        /// <summary>
        ///     Used to clear any pending exception reads leaving the exception in the queue
        /// </summary>
        void ClearPendingException();

        /// <summary>
        ///     Removes the requested exception from the queue.
        ///     This is useful for priority exceptions which may no longer be valid
        /// </summary>
        /// <param name="exception">The exception to remove from the queue</param>
        void RemoveException(ISasExceptionCollection exception);
    }
}
