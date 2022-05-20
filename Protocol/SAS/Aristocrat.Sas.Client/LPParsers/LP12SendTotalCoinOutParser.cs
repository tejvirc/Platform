namespace Aristocrat.Sas.Client.LPParsers
{
    using Client;

    /// <summary>
    ///     This handles the Send Total Coin Out Meter Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           12        Send Total Coin Out Meter
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           12        Send Total Coin Out Meter
    /// Meter         4 BCD 00000000-99999999  four byte BCD total coin out meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP12SendTotalCoinOutParser : SasSingleMeterLongPollParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of the LP12SendTotalCoinOutParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP12SendTotalCoinOutParser(SasClientConfiguration configuration)
            : base(LongPoll.SendCoinOutMeter, SasMeters.TotalCoinOut, MeterType.Lifetime, configuration, true)
        {
        }
    }
}
