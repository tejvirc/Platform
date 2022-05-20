namespace Aristocrat.Sas.Client.LPParsers
{
    /// <summary>
    ///     This handles the Send Selected Meters For Game N Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           2F        Send Selected Meters For Game N
    /// Length           1         03-0C       number of bytes following not including the CRC
    /// Game Number    2 BCD      0000-9999    The game number (0000 = gaming machine)
    /// Requested Meter  1         00-FF       Meter code for first requested meter. See Table C-7 in Appendix C for codes
    /// ....           variable    ....        Optional additional meter codes (10 meters maximum per command)
    /// CRC              2        0000-FFFF    16-bit CRC
    /// 
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           2F        Send Selected Meters For Game N
    /// Length           1         02-3E       number of remaining bytes not counting the CRC
    /// Game Number    2 BCD      0000-9999    The game number
    /// Meter code       1          00-FF      Meter code from Table C-7 in Appendix C of SAS Protocol spec
    /// Meter Value    n BCD         ???       meter value. (use Min Size from Table C-7)
    /// optional repeat ....                   optional additional meters being reported
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP2FSendSelectedMetersForGameNParser : SendSelectedMetersForGameNParserBase
    {
        /// <inheritdoc />
        public LP2FSendSelectedMetersForGameNParser(SasClientConfiguration configuration)
            : base(LongPoll.SendSelectedMetersForGameN, false, configuration)
        {
        }
    }
}