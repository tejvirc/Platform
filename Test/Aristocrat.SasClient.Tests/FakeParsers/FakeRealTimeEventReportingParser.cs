namespace Aristocrat.SasClient.Tests.FakeParsers
{
    using System.Collections.Generic;
    using Sas.Client;

    [Sas(SasGroup.PerClientLoad)]
    public class FakeRealTimeEventReportingParser : SasLongPollParser<LongPollResponse, LongPollData>
    {
        /// <summary>
        ///     Creates a new instance of a fake parser for use in the SasLongPollParserFactoryTest
        /// </summary>
        public FakeRealTimeEventReportingParser()
            : base(LongPoll.EnableDisableRealTimeEventReporting)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            // fake return value for testing
            return new List<byte> { 0x11, 0x12 };
        }
    }
}