namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Client;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Legacy Bonus Meters Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           9A        Send Legacy Bonus Meters
    /// Game Number    2 BCD      0000-9999    Game Number
    /// CRC              2        0000-FFFF    16-bit CRC
    /// 
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           9A        Send Legacy Bonus Meters
    /// Game Number    2 BCD      0000-9999    Game Number
    /// Deductible     4 BCD 00000000-99999999 four byte BCD deductible bonus meter value for game N
    /// Non-Deductible 4 BCD 00000000-99999999 four byte BCD non-deductible bonus meter value for game N
    /// Wager Match    4 BCD 00000000-99999999 four byte BCD wager match bonus meter value for game N
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP9ASendLegacyBonusMetersParser : SasLongPollParser<LongPollSendLegacyBonusMetersResponse, LongPollSendLegacyBonusMetersData>
    {
        private const int GameNumberOffset = 2;
        private const int GameNumberLength = 2;
        private const byte BytesToCopyFromCommandToResponse = 4;

        /// <summary>
        ///     Instantiates a new instance of the LP9ASendLegacyBonusMetersParser class
        /// </summary>
        public LP9ASendLegacyBonusMetersParser(SasClientConfiguration configuration)
            : base(LongPoll.SendLegacyBonusMeters)
        {
            Data.AccountingDenom = configuration.AccountingDenom;
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Logger.Debug("LongPoll(9A) Send Legacy Bonus Meters");
            var longPoll = command.ToArray();

            var (id, valid) = Utilities.FromBcdWithValidation(longPoll, GameNumberOffset, GameNumberLength);

            if (!valid)
            {
                Logger.Debug("Game Id not valid BCD. NACKing Send Legacy Bonus Meters long poll.");
                return NackLongPoll(command);
            }

            Data.GameId = (int)id;

            var result = Handle(Data);

            var response = new List<byte>(command.Take(BytesToCopyFromCommandToResponse).ToList());
            response.AddRange(Utilities.ToBcd(result.Deductible, SasConstants.Bcd8Digits));
            response.AddRange(Utilities.ToBcd(result.NonDeductible, SasConstants.Bcd8Digits));
            response.AddRange(Utilities.ToBcd(result.WagerMatch, SasConstants.Bcd8Digits));

            return response;
        }
    }
}
