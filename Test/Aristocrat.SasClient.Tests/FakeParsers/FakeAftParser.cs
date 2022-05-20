namespace Aristocrat.SasClient.Tests.FakeParsers
{
    using System.Collections.Generic;
    using Sas.Client;

    [Sas(SasGroup.Aft)]
    public class FakeAftParser : SasLongPollParser<LongPollResponse, LongPollData>
    {
        /// <summary>
        ///     Creates a new instance of a fake parser for use in the SasLongPollParserFactoryTest
        /// </summary>
        public FakeAftParser()
            : base(LongPoll.AftTransferFunds)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            // fake return value for testing
            return new List<byte> { 0x33, 0x34 };
        }
    }
}