namespace Aristocrat.Sas.Client.LPParsers
{
    using Client;

    /// <summary>
    ///     This handles the Send $200 Bills In Meter Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           3A        Send $200 Bills In Meter
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           3A        Send $200 Bills In Meter
    /// Meter         4 BCD 00000000-99999999  four byte BCD meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP3ASend200DollarBillsInParser : SasSingleMeterLongPollParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of the LP3ASend200DollarBillsInParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP3ASend200DollarBillsInParser(SasClientConfiguration configuration)
            : base(LongPoll.SendTwoHundredDollarInMeter, SasMeters.DollarsIn200, MeterType.Lifetime, configuration)
        {
        }
    }
}