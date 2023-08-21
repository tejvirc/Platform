namespace Aristocrat.Sas.Client.LPParsers
{
    using Client;

    /// <summary>
    ///     This handles the Send Total Bill In Value Meter Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           20        Send Total Bill In Value Meter
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           20        Send Total Bill In Value Meter
    /// Meter         4 BCD 00000000-99999999  four byte BCD total drop meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP20TotalBillInValueParser : SasSingleMeterLongPollParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of the LP20TotalBillInValueParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP20TotalBillInValueParser(SasClientConfiguration configuration)
            : base(LongPoll.SendTotalBillInValueMeter, SasMeters.TotalBillIn, MeterType.Lifetime, configuration)
        {
        }
    }
}
