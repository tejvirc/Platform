namespace Aristocrat.Sas.Client.LPParsers
{
    using Client;

    /// <summary>
    ///     This handles the Send Total Jackpot Meter Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           14        Send Total Jackpot Meter
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           14        Send Total Jackpot Meter
    /// Meter         4 BCD 00000000-99999999  four byte BCD total jackpot meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP14SendTotalJackpotParser : SasSingleMeterLongPollParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of the LP14SendTotalJackpotParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP14SendTotalJackpotParser(SasClientConfiguration configuration)
            : base(LongPoll.SendJackpotMeter, SasMeters.TotalJackpot, MeterType.Lifetime, configuration, true)
        {
        }
    }
}
