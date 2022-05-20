namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Cash Out Limit Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           A4        Send Cash Out Limit
    /// Game Number    2 BCD        XXXX       Game number (0000 = gaming machine)
    /// CRC              2       0000-FFFF     16-bit CRC
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           A4        Send Cash Out Limit
    /// Game Number    2 BCD        XXXX       Game number
    /// Cash Out Limit 2 BCD                   Cash out limit in SAS accounting denom units, send MSB first
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LPA4SendCashOutLimitParser : SasLongPollParser<LongPollReadSingleValueResponse<ulong>, SendCashOutLimitData>
    {
        private const int CommandAndGameIdSize = 4;
        private const int GameIdPos = 2;
        private const int GameIdByteSize = 2;

        /// <summary>
        ///     Instantiates a new instance of the LPA4SendCashOutLimitParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LPA4SendCashOutLimitParser(SasClientConfiguration configuration)
            : base(LongPoll.SendCashOutLimit)
        {
            Data.AccountingDenom = configuration.AccountingDenom;
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var result = command.Take(CommandAndGameIdSize).ToList();
            var (gameNum, valid) = Utilities.FromBcdWithValidation(command.ToArray(), GameIdPos, GameIdByteSize);
            if (!valid)
            {
                Logger.Debug("Game Id not valid BCD. NACKing send game N configuration long poll");
                return NackLongPoll(command);
            }

            Data.GameId = (int)gameNum;
            result.AddRange(Utilities.ToBcd(Handler(Data).Data, SasConstants.Bcd4Digits));

            return result;
        }
    }
}
