namespace Aristocrat.SasClient.Tests.FakeParsers
{
    using System.Collections.Generic;
    using Sas.Client;

    [Sas(SasGroup.LegacyBonus)]
    public class FakeLegacyBonusParser : SasLongPollParser<LongPollResponse, LongPollData>
    {
        /// <summary>
        ///     Creates a new instance of a fake parser for use in the SasLongPollParserFactoryTest
        /// </summary>
        public FakeLegacyBonusParser()
            : base(LongPoll.InitiateLegacyBonusPay)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            // fake return value for testing
            return new List<byte> { 0x44, 0x45 };
        }
    }
}