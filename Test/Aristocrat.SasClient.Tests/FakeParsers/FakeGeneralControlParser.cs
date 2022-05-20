namespace Aristocrat.SasClient.Tests.FakeParsers
{
    using System.Collections.Generic;
    using Sas.Client;

    [Sas(SasGroup.GeneralControl)]
    public class FakeGeneralControlParser : SasLongPollParser<LongPollResponse, LongPollData>
    {
        /// <summary>
        ///     Creates a new instance of a fake parser for use in the SasLongPollParserFactoryTest
        /// </summary>
        public FakeGeneralControlParser()
            : base(LongPoll.EnableDisableGameN)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            // fake return value for testing
            return new List<byte> { 0x22, 0x23 };
        }
    }
}