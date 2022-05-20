namespace Aristocrat.SasClient.Tests.FakeParsers
{
    using Sas.Client;

    [Sas(SasGroup.GeneralControl)]
    public class FakeReadSingleMeterParser : SasSingleMeterLongPollParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of a FakeReadSingleMeterParser class
        /// </summary>
        public FakeReadSingleMeterParser()
            : base(
                LongPoll.SendCanceledCreditsMeter,
                SasMeters.TotalCanceledCredits,
                MeterType.Lifetime,
                new SasClientConfiguration())
        {
        }
    }
}

