namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Client;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Legacy Bonus Award Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value             Description
    /// Address          1         01-7F             Gaming Machine Address
    /// Command          1           8A              Initiate a legacy bonus pay
    /// Bonus Amount   4 BCD    00000000-99999999    Bonus Amount in SAS accounting denom units
    /// Tax status       1         00-02             00-Deductible, 01-Non-deductible, 02-Wager match
    /// CRC              2        0000-FFFF          16-bit CRC
    /// 
    /// Response
    /// Ack or Nack
    /// </remarks>
    [Sas(SasGroup.LegacyBonus)]
    public class LP8ALegacyBonusAwardsParser : SasLongPollParser<LongPollReadSingleValueResponse<byte>, LegacyBonusAwardsData>
    {
        private const int BonusAmountOffset = 2;
        private const int TaxStatusOffset = 6;
        private const byte RespondIgnore = 0;
        private const byte RespondAck = 1;

        /// <summary>
        ///     Instantiates a new instance of the LP8ALegacyBonusAwardsParser class
        /// </summary>
        public LP8ALegacyBonusAwardsParser(SasClientConfiguration configuration)
            : base(LongPoll.InitiateLegacyBonusPay)
        {
            Data.AccountingDenom = configuration.AccountingDenom;
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Logger.Debug("LongPoll(8A) Initiate Legacy Bonus Pay");
            var longPoll = command.ToArray();

            var (bonusAmount, validBonus) = Utilities.FromBcdWithValidation(longPoll, BonusAmountOffset, SasConstants.Bcd8Digits);
            if (!validBonus)
            {
                Logger.Debug("Legacy Bonus Award Amount is not a valid BCD number");
                return NackLongPoll(command);
            }
            Data.BonusAmount = (long)bonusAmount;

            var taxStatus = longPoll[TaxStatusOffset];
            // check for valid values for tax status
            if (taxStatus > (byte)TaxStatus.WagerMatch)
            {
                Logger.Debug("Tax Status out of range. NACKing Legacy Bonus Award long poll");
                return NackLongPoll(command);
            }
            Data.TaxStatus = (TaxStatus)taxStatus;

            var result = Handle(Data);

            return (result.Data == RespondAck) ? AckLongPoll(command) : ((result.Data == RespondIgnore) ? null : BusyResponse(command));
        }
    }
}
