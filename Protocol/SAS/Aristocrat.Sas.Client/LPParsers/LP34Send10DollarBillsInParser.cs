namespace Aristocrat.Sas.Client.LPParsers
{
    using Client;

    /// <summary>
    ///     This handles the Send $10 Bills In Meter Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           34        Send $10 Bills In Meter
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           34        Send $10 Bills In Meter
    /// Meter         4 BCD 00000000-99999999  four byte BCD meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP34Send10DollarBillsInParser : SasSingleMeterLongPollParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of the LP34Send10DollarBillsInParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP34Send10DollarBillsInParser(SasClientConfiguration configuration)
            : base(LongPoll.SendTenDollarInMeter, SasMeters.DollarsIn10, MeterType.Lifetime, configuration)
        {
        }
    }
}