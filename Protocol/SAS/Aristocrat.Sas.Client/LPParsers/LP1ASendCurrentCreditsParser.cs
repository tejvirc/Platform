namespace Aristocrat.Sas.Client.LPParsers
{
    using Client;

    /// <summary>
    ///     This parses the Send Current Credits Meter Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           1A        Send Current Credits Meter
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           1A        Send Current Credits Meter
    /// Meter         4 BCD 00000000-99999999  four byte BCD canceled credits meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP1ASendCurrentCreditsParser : SasSingleMeterLongPollParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of the LP1ASendCurrentCreditsParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP1ASendCurrentCreditsParser(SasClientConfiguration configuration)
            : base(LongPoll.SendCurrentCredits, SasMeters.CurrentCredits, MeterType.Lifetime, configuration)
        {
        }
    }
}
