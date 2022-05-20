namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Client;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Game N Configuration Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           53        Send Game N Configuration
    /// Game Number    2 BCD        XXXX       Game number (0000 = gaming machine)
    /// CRC              2       0000-FFFF     16-bit CRC
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           53        Send Game N Configuration
    /// Game number    2 BCD      0000-9999    Game number (0000 = gaming machine) 
    /// Game ID       2 ASCII        ??        Game ID in ASCII
    /// Additional ID 3 ASCII        ??        Additional Game ID in ASCII. If game doesn't support additional ID this should return "000"
    /// Denomination     1          00-FF      The SAS accounting denomination of this gaming machine. See Table C-4 in Appendix C.
    /// Max Bet          1          01-FF      Largest configured MAX bet for the gaming machine or FF if max bet is greater than 255
    /// Prog Group       1          00-FF      Current configured progressive group
    /// Game Options     2        0000-FFFF    Game options. See Table C-2 in Appendix C.
    /// Paytable ID   6 ASCII       ??????     Paytable ID. See table C-3 in Appendix C
    /// Base %        4 ASCII        ????      Theoretical RTP percentage at max bet. The decimal is implied and NOT transmitted.
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP53SendGameNConfigurationParser : SendGameNConfigurationParserBase
    {
        private const int GameNumberIndex = 2;
        private const int GameNumberLength = 2;

        /// <summary>
        ///     Instantiates a new instance of the LP53SendGameNConfigurationParser class
        /// </summary>
        public LP53SendGameNConfigurationParser(SasClientConfiguration configuration)
            : base(LongPoll.SendGameNConfiguration, configuration)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var longPoll = command.ToArray();
            var (id, valid) = Utilities.FromBcdWithValidation(longPoll, GameNumberIndex, GameNumberLength);

            if (!valid)
            {
                Logger.Debug("Game Id not valid BCD. NACKing send game N configuration long poll");
                return NackLongPoll(command);
            }

            Data.GameNumber = id;
            return base.Parse(command);
        }

        protected override IReadOnlyCollection<byte> GetResponseData(LongPollMachineIdAndInfoResponse handlerResponse)
        {
            var response = new List<byte>();
            response.AddRange(Utilities.ToBcd(Data.GameNumber, GameNumberLength));
            response.AddRange(base.GetResponseData(handlerResponse));
            return response;
        }
    }
}
