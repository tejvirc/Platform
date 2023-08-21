namespace Aristocrat.Sas.Client.LPParsers
{
    using Client;

    /// <summary>
    ///     This parses the Send Credit Amount of bills in stacker Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           4A        Send Credit Amount of bills in stacker
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           4A        Send Credit Amount of bills in stacker
    /// Meter         4 BCD 00000000-99999999  four byte BCD canceled credits meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP4ASendCreditAmountOfBillsInStackerParser : SasSingleMeterLongPollParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of the LP4ASendCreditAmountOfBillsInStackerParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP4ASendCreditAmountOfBillsInStackerParser(SasClientConfiguration configuration)
            : base(LongPoll.SendCreditAmountOfBillsInStacker, SasMeters.TotalCreditOfBillsInStacker, MeterType.Period, configuration)
        {
        }
    }
}
