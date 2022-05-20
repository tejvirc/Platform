namespace Aristocrat.Sas.Client.LPParsers
{
    using Client;

    /// <summary>
    ///     This handles the Send True Coin In Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           2A        Send True Coin In
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           2A        Send True Coin In
    /// Meter         4 BCD 00000000-99999999  four byte BCD true coin in meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP2ASendTrueCoinInParser : SasSingleMeterLongPollParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of the LP2ASendTrueCoinInParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP2ASendTrueCoinInParser(SasClientConfiguration configuration)
            : base(LongPoll.SendTrueCoinIn, SasMeters.TrueCoinIn, MeterType.Lifetime, configuration)
        {
        }
    }
}