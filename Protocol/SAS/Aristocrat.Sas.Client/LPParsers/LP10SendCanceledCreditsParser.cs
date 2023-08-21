namespace Aristocrat.Sas.Client.LPParsers
{
    using Client;

    /// <summary>
    ///     This parses the Send Total Canceled Credits Meter Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           10        Send Total Canceled Credits Meter
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           10        Send Total Canceled Credits Meter
    /// Meter         4 BCD 00000000-99999999  four byte BCD canceled credits meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP10SendCanceledCreditsParser : SasSingleMeterLongPollParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of the LP10SendCanceledCreditsParser class
        /// </summary>
        public LP10SendCanceledCreditsParser(SasClientConfiguration configuration)
            : base(LongPoll.SendCanceledCreditsMeter, SasMeters.TotalCanceledCredits, MeterType.Lifetime, configuration)
        {
        }
    }
}
