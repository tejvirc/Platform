namespace Aristocrat.Monaco.Sas.Handlers
{
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     The handler for LP 4F Send Current Hopper Status
    /// </summary>
    /// <inheritdoc />
    public class LP4FSendCurrentHopperStatusHandler : ISasLongPollHandler<LongPollHopperStatusResponse, LongPollData>
    {
        private const byte UnknownPercentFull = 0xFF;

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll> { LongPoll.SendCurrentHopperStatus };

        /// <inheritdoc />
        public LongPollHopperStatusResponse Handle(LongPollData data)
        {
            // TODO: Use actual device statuses once implemented
            return new LongPollHopperStatusResponse
            {
                Level = 0,
                PercentFull = UnknownPercentFull,
                Status = LongPollHopperStatusResponse.HopperStatus.HopperEmpty
            };
        }
    }
}