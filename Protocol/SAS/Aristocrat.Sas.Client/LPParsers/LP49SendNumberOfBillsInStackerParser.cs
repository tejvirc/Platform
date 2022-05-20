namespace Aristocrat.Sas.Client.LPParsers
{
    using Client;

    /// <summary>
    ///     This handles the Send Number of Bills In Stacker Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           49        Send Number of Bills In Stacker
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           49        Send Number of Bills In Stacker
    /// Meter         4 BCD 00000000-99999999  four byte BCD number of bills count
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP49SendNumberOfBillsInStackerParser : SasSingleMeterLongPollParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of the LP49SendNumberOfBillsInStackerParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP49SendNumberOfBillsInStackerParser(SasClientConfiguration configuration)
            : base(LongPoll.SendNumberOfBillsInStacker, SasMeters.NumberOfBillsInStacker, MeterType.Period, configuration)
        {
        }
    }
}