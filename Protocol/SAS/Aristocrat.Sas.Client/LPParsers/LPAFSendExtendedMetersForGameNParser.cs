namespace Aristocrat.Sas.Client.LPParsers
{
    /// <summary>
    ///     This handles the Send Extended Meters for Game N Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           AF        Send Extended Meters for Game N
    /// Length           1         04-1A       number of bytes following not including the CRC
    /// Game Number    2 BCD      0000-9999    The game number (0000 = gaming machine)
    /// Requested Meter  2        0000-FFFF    Meter code for first requested meter. See Table C-7 in Appendix C for codes
    /// ....           variable    ....        Optional additional meter codes (10 meters maximum per command)
    /// CRC              2       0000-FFFF     16-bit CRC
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           AF        Send Extended Meters for Game N
    /// Length           1         05-nn       number of remaining bytes not counting the CRC
    /// Game Number    2 BCD      0000-9999    The game number
    /// Meter code       2        0000-FFFF    Meter code from Table C-7 in Appendix C of SAS Protocol spec
    /// Meter Size       1          00-09      Meter size in bytes
    /// Meter Value    n BCD         ???       meter value (0-9 bytes)
    /// optional repeat ....                   optional additional meters being reported
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LPAFSendExtendedMetersForGameNParser : SendSelectedMetersForGameNParserBase
    {
        /// <inheritdoc />
        public LPAFSendExtendedMetersForGameNParser(SasClientConfiguration configuration)
            : base(LongPoll.SendExtendedMetersForGameNAlternate, true, configuration)
        {
        }
    }
}