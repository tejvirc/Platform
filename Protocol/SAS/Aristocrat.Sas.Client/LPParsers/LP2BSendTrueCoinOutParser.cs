namespace Aristocrat.Sas.Client.LPParsers
{
    using Client;

    /// <summary>
    /// This handles the Send True Coin Out Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           2B        Send True Coin Out
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           2B        Send True Coin Out
    /// Meter         4 BCD 00000000-99999999  four byte BCD true coin out meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP2BSendTrueCoinOutParser : SasSingleMeterLongPollParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of the LP2BSendTrueCoinOutParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP2BSendTrueCoinOutParser(SasClientConfiguration configuration)
            : base(LongPoll.SendTrueCoinOut, SasMeters.TrueCoinOut, MeterType.Lifetime, configuration)
        {
        }
    }
}