namespace Aristocrat.Sas.Client
{
    using System.Collections.Generic;

    public interface ILongPollParser
    {
        /// <summary>
        /// Gets the long poll command
        /// </summary>
        LongPoll Command { get; }

        /// <summary>
        /// Contains any handlers the HostAcknowledgementProvider should call
        /// </summary>
        IHostAcknowledgementHandler Handlers { get; }

        /// <summary>
        /// Parses the long poll and gets a response
        /// </summary>
        /// <param name="command">the long poll message</param>
        /// <returns>The response for the long poll or
        /// null if there isn't a response</returns>
        IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command);

        /// <summary>
        /// Injects a handler from the platform 
        /// </summary>
        /// <param name="handler">The handler used to process the long poll message</param>
        void InjectHandler(object handler);
    }
}