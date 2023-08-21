namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Client;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Legacy Bonus Win Amount Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           90        Send Legacy Bonus Win Amount
    ///
    /// Response
    /// Field          Bytes       Value             Description
    /// Address          1         01-7F             Gaming Machine Address
    /// Command          1           90              Send Legacy Bonus Win Amount
    /// Multiplier       1       00-0A/81-8A         Binary multiplier
    /// Multiplied Win  4 BCD    00000000-99999999   Multiplied Win Amount in SAS accounting denom units
    /// Tax status       1         00-02             00-Deductible, 01-Non-deductible, 02-Wager match
    /// Bonus           4 BCD    00000000-99999999   Legacy Bonus Win Amount in SAS accounting denom units
    /// CRC              2        0000-FFFF          16-bit CRC
    /// </remarks>
    [Sas(SasGroup.LegacyBonus)]
    public class LP90SendLegacyBonusWinAmountParser : SasLongPollParser<LegacyBonusWinAmountResponse, LongPollSingleValueData<long>>
    {
        /// <summary>
        ///     Instantiates a new instance of the LP90SendLegacyBonusWinAmountParser class
        /// </summary>
        public LP90SendLegacyBonusWinAmountParser(SasClientConfiguration configuration)
            : base(LongPoll.SendLegacyBonusWinAmount)
        {
            Data.Value = configuration.AccountingDenom;
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Logger.Debug("LongPoll(90) Send Legacy Bonus Win Amount");
            var response = new List<byte>(command.Take(2).ToList());

            var result = Handle(Data);

            Handlers = result.Handlers;
            response.Add(result.Multiplier);
            response.AddRange(Utilities.ToBcd(result.MultipliedWin, SasConstants.Bcd8Digits));
            response.Add((byte)result.TaxStatus);
            response.AddRange(Utilities.ToBcd(result.BonusAmount, SasConstants.Bcd8Digits));

            return response;
        }
    }
}
