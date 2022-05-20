namespace Aristocrat.Sas.Client.LPParsers
{
    using Client;

    /// <summary>
    ///     This handles the Send $50 Bills In Meter Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           36        Send $50 Bills In Meter
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           36        Send $50 Bills In Meter
    /// Meter         4 BCD 00000000-99999999  four byte BCD meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP36Send50DollarBillsInParser : SasSingleMeterLongPollParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of the LP36Send50DollarBillsInParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP36Send50DollarBillsInParser(SasClientConfiguration configuration)
            : base(LongPoll.SendFiftyDollarInMeter, SasMeters.DollarsIn50, MeterType.Lifetime, configuration)
        {
        }
    }
}