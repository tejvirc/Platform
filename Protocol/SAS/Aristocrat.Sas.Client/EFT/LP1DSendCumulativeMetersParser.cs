namespace Aristocrat.Sas.Client.Eft
{
    using System.Collections.Generic;
    using System.Linq;
    using EFT;

    /// <summary>
    ///     (From section 8.EFT of the SAS v5.02 document)  -
    ///     https://confy.aristocrat.com/pages/viewpage.action?pageId=159599156
    ///     EFT Long poll parser for handling 0x1D request, sending of cumulative meters to the host.
    /// </summary>
    [Sas(SasGroup.Eft)]
    public class LP1DSendCumulativeMetersParser : SasLongPollParser<CumulativeEftMeterData, LongPollData>
    {
        /// <summary>
        ///     Instantiates a new instance of the LP1DSendCumulativeMeters class
        /// </summary>
        public LP1DSendCumulativeMetersParser()
            : base(LongPoll.EftSendCumulativeMeters)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var response = Handle(Data);
            return GenerateResponseBytes(response, command);
        }

        private static List<byte> GenerateResponseBytes(
            CumulativeEftMeterData response,
            IReadOnlyCollection<byte> command)
        {
            var responseByte = new List<byte>();
            responseByte.AddRange(command.Take(2));
            responseByte.AddRange(Utilities.ToBcd(response.PromotionalCredits, SasConstants.Bcd8Digits));
            responseByte.AddRange(Utilities.ToBcd(response.NonCashableCredits, SasConstants.Bcd8Digits));
            responseByte.AddRange(Utilities.ToBcd(response.TransferredCredits, SasConstants.Bcd8Digits));
            responseByte.AddRange(Utilities.ToBcd(response.CashableCredits, SasConstants.Bcd8Digits));
            return responseByte;
        }
    }
}