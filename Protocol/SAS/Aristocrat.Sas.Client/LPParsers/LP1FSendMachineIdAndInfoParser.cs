namespace Aristocrat.Sas.Client.LPParsers
{
    /// <summary>
    ///     This handles the Send Gaming Machine Id Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           1F        Send Gaming Machine Id
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           1F        Send Gaming Machine Id
    /// Game ID       2 ASCII        ??        Game ID in ASCII
    /// Additional ID 3 ASCII        ??        Additional Game ID in ASCII. If game doesn't support additional ID this should return "000"
    /// Denomination     1          00-FF      The SAS accounting denomination of this gaming machine. See Table C-4 in Appendix C.
    /// Max Bet          1          01-FF      Largest configured MAX bet for the gaming machine or FF if max bet is greater than 255
    /// Prog Group       1          00-FF      Current configured progressive group
    /// Game Options     2        0000-FFFF    Game options. See Table C-2 in Appendix C.
    /// Paytable ID   6 ASCII       ??????     Paytable ID. See table C-3 in Appendix C
    /// Base %        4 ASCII        ????      Theoretical RTP percentage at max bet. The decimal is implied and NOT transmitted.
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP1FSendMachineIdAndInfoParser : SendGameNConfigurationParserBase
    {
        /// <summary>
        ///     Instantiates a new instance of the LP1FSendMachineIdAndInfoParser class
        /// </summary>
        public LP1FSendMachineIdAndInfoParser(SasClientConfiguration configuration)
            : base(LongPoll.SendMachineIdAndInformation, configuration)
        {
            Data.GameNumber = 0;
        }
    }
}
