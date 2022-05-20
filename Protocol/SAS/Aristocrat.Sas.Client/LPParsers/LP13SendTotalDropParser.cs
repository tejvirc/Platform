namespace Aristocrat.Sas.Client.LPParsers
{
    using Client;

    /// <summary>
    ///     This handles the Send Total Drop Meter Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           13        Send Total Drop Meter
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           13        Send Total Drop Meter
    /// Meter         4 BCD 00000000-99999999  four byte BCD total drop meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP13SendTotalDropParser : SasSingleMeterLongPollParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of the LP13SendTotalDropParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP13SendTotalDropParser(SasClientConfiguration configuration)
            : base(LongPoll.SendDropMeter, SasMeters.TotalDrop, MeterType.Lifetime, configuration)
        {
        }
    }
}
