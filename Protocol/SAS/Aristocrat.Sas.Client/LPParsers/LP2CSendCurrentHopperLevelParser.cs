namespace Aristocrat.Sas.Client.LPParsers
{
    using Client;

    /// <summary>
    ///     This handles the Send Current Hopper Level Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           2C        Send Current Hopper Level
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           2C        Send Current Hopper Level
    /// Meter         4 BCD 00000000-99999999  four byte BCD hopper level value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP2CSendCurrentHopperLevelParser : SasSingleMeterLongPollParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of the LP2CSendCurrentHopperLevelParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP2CSendCurrentHopperLevelParser(SasClientConfiguration configuration)
            : base(LongPoll.SendCurrentHopperLevel, SasMeters.CurrentHopperLevel, MeterType.Lifetime, configuration)
        {
        }
    }
}