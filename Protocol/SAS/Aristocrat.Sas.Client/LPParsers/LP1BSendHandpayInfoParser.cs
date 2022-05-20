namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Handpay Information Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           1B        Send Handpay Information
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           1B        Send Handpay Information
    /// Prog Group       1         00-FF       Progressive group. 00 = Stand alone, non, or linked progressive
    ///                                          01-FF = Sas controlled progressive
    /// Level            1         00-80       Level of highest contributing progressive win for the handpay. 01=highest, 20=lowest
    ///                                          40 = non progressive win
    ///                                          80 = canceled credits amount
    /// Amount        5 BCD        XXXXX       Total amount of handpay
    /// Partial pay   2 BCD          XX        Any partial amount paid prior to jackpot handpay
    /// Reset ID        1          00-01       Available reset methods. 00=Only operator key off, 01=pay to credit meter
    /// Unused         10            0         Reserved for future use
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP1BSendHandpayInfoParser : SasLongPollParser<LongPollHandpayDataResponse, LongPollHandpayData>
    {
        /// <summary>
        ///     Instantiates a new instance of the LP1BSendHandpayInfoParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP1BSendHandpayInfoParser(SasClientConfiguration configuration)
            : base(LongPoll.SendHandpayInformation)
        {
            Data.ClientNumber = configuration.ClientNumber;
            Data.AccountingDenom = configuration.AccountingDenom;
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var result = command.ToList();
            var response = Handler(Data);

            Handlers = response.Handlers;
            result.Add((byte)response.ProgressiveGroup);
            result.Add((byte)response.Level);
            result.AddRange(Utilities.ToBcd((ulong)response.Amount, SasConstants.Bcd10Digits));
            result.AddRange(Utilities.ToBcd((ulong)response.PartialPayAmount, SasConstants.Bcd4Digits));
            result.Add((byte)response.ResetId);
            result.AddRange(Utilities.ToBcd((ulong)response.SessionGameWinAmount, SasConstants.Bcd10Digits));
            result.AddRange(Utilities.ToBcd((ulong)response.SessionGamePayAmount, SasConstants.Bcd10Digits));
            return result;
        }
    }
}
