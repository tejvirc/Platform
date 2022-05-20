namespace Aristocrat.Sas.Client.LPParsers
{
    using Client;

    /// <summary>
    ///     This handles the Send Total Coin In Meter Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           11        Send Total Coin In Meter
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           11        Send Total Coin In Meter
    /// Meter         4 BCD 00000000-99999999  four byte BCD total coin in meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP11SendTotalCoinInParser : SasSingleMeterLongPollParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of the LP11SendTotalCoinInParser class
        /// </summary>
        public LP11SendTotalCoinInParser(SasClientConfiguration configuration)
            : base(LongPoll.SendCoinInMeter, SasMeters.TotalCoinIn, MeterType.Lifetime, configuration,true)
        {
        }
    }
}
