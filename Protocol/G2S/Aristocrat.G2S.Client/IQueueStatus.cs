namespace Aristocrat.G2S.Client
{
    using System;

    /// <summary>
    ///     Provides a mechanism to get the status of a queue
    /// </summary>
    public interface IQueueStatus
    {
        /// <summary>
        ///     Gets a value indicating whether the queue is online.
        /// </summary>
        /// <returns><b>true</b> if online; otherwise <b>false</b>.</returns>
        bool Online { get; }

        /// <summary>
        ///     Gets a value indicating whether get a flag indicating if sending is enabled.
        /// </summary>
        /// <returns><b>true</b> if successful; otherwise <b>false</b>.</returns>
        bool CanSend { get; }

        /// <summary>
        ///     Gets the count of <see cref="T:Aristocrat.G2S.ClassCommand" />s in the received queue.
        /// </summary>
        /// <returns>The count of commands in the received queue.</returns>
        int ReceivedCount { get; }

        /// <summary>
        ///     Gets the count of <see cref="T:Aristocrat.G2S.ClassCommand" />s in the send queue.
        /// </summary>
        /// <returns>The count of commands in the send queue.</returns>
        int SendCount { get; }

        /// <summary>
        ///     Gets the time elapsed since last received command (specifically on the communication class).
        /// </summary>
        /// <returns>The date/time of the last command received.</returns>
        TimeSpan ReceivedElapsedTime { get; }

        /// <summary>
        ///     Gets the time elapsed since the last sent command (specifically on the communication class).
        /// </summary>
        /// <returns>The date/time of the last command sent.</returns>
        TimeSpan SentElapsedTime { get; }

        /// <summary>
        ///     Gets a value indicating whether the outbound queue is full.
        /// </summary>
        /// <returns>True if the outbound queue is full.</returns>
        bool OutboundQueueFull { get; }

        /// <summary>
        ///     Gets a value indicating whether the inbound queue is full.
        /// </summary>
        /// <returns>True if the inbound queue is full.</returns>
        bool InboundQueueFull { get; }
    }
}