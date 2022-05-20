namespace Aristocrat.Sas.Client.LPParsers
{
    using Client;

    /// <summary>
    ///     This handles the Send $25,000 Bills In Meter Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           42        Send $25,000 Bills In Meter
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           42        Send $25,000 Bills In Meter
    /// Meter         4 BCD 00000000-99999999  four byte BCD meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP42Send25000DollarBillsInParser : SasSingleMeterLongPollParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of the LP42Send25000DollarBillsInParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP42Send25000DollarBillsInParser(SasClientConfiguration configuration)
            : base(LongPoll.SendTwentyFiveThousandDollarInMeter, SasMeters.DollarsIn25000, MeterType.Lifetime, configuration)
        {
        }
    }
}