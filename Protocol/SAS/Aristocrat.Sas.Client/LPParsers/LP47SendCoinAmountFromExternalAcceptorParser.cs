namespace Aristocrat.Sas.Client.LPParsers
{
    using Client;

    /// <summary>
    ///     This handles the Send Coin Amount Accepted From An External Coin Acceptor Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           47        Send Coin Amount Accepted From An External Coin Acceptor
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           47        Send Coin Amount Accepted From An External Coin Acceptor
    /// Meter         4 BCD 00000000-99999999  four byte BCD coin amount meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP47SendCoinAmountFromExternalAcceptorParser : SasSingleMeterLongPollParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of the LP47SendCoinAmountFromExternalAcceptorParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP47SendCoinAmountFromExternalAcceptorParser(SasClientConfiguration configuration)
            : base(LongPoll.SendCoinAcceptedFromExternalAcceptor, SasMeters.CoinAmountAcceptedFromExternal, MeterType.Lifetime, configuration)
        {
        }
    }
}