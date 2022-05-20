namespace Aristocrat.Sas.Client.LPParsers
{
    using Client;

    /// <summary>
    ///     This handles the Send Games Lost Meter Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           17        Send Games Lost Meter
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           17        Send Games Lost Meter
    /// Meter         4 BCD 00000000-99999999  four byte BCD games lost meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP17SendGamesLostParser : SasSingleMeterLongPollParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of the LP17SendGamesLostParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP17SendGamesLostParser(SasClientConfiguration configuration)
            : base(LongPoll.SendGamesLostMeter, SasMeters.GamesLost, MeterType.Lifetime, configuration, true)
        {
        }
    }
}
