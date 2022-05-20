namespace Aristocrat.Sas.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class UnhandledLongPollParser : SasLongPollParser<LongPollResponse, LongPollData>
    {
        /// <summary>
        /// Instantiates a new instance of the UnhandledLongPollParser class
        /// </summary>
        public UnhandledLongPollParser() : base(LongPoll.None)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var longPoll = command.ToArray();
            Logger.Debug($"UnhandledLongPollParser called. Command is {BitConverter.ToString(longPoll)}. Long poll is {(LongPoll)longPoll[SasConstants.SasCommandIndex]}");
            return null;
        }

        /// <inheritdoc/>
        public override void InjectHandler(object handler)
        {
            // we would only get here if we had a handler for a long poll but no associated parser
            // do nothing since we don't have a parser for this long poll
        }
    }
}
