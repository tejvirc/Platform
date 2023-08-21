namespace Aristocrat.SasClient.Tests.FakeParsers
{
    using System.Collections.Generic;
    using Sas.Client;

    /// <summary>
    ///     Tests that the SasLongPollParserFactory doesn't load this parser since it doesn't have attributes
    /// </summary>
    public class FakeParserWithoutAttribute : SasLongPollParser<LongPollResponse, LongPollData>
    {
        /// <summary>
        ///     Creates a new instance of a fake parser for use in the SasLongPollParserFactoryTest
        /// </summary>
        public FakeParserWithoutAttribute()
            : base(LongPoll.DelayGame)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            // fake return value for testing
            return new List<byte> { 0x77, 0x78 };
        }
    }
}