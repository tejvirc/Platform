namespace Aristocrat.Sas.Client.LPParsers
{
    using Client;

    /// <summary>
    ///     This handles the Send Games Won Meter Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           16        Send Games Won Meter
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           16        Send Games Won Meter
    /// Meter         4 BCD 00000000-99999999  four byte BCD games won meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP16SendGamesWonParser : SasSingleMeterLongPollParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of the LP16SendGamesWonParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP16SendGamesWonParser(SasClientConfiguration configuration)
            : base(LongPoll.SendGamesWonMeter, SasMeters.GamesWon, MeterType.Lifetime, configuration, true)
        {
        }
    }
}
