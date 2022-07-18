namespace Aristocrat.Sas.Client.Eft
{
    using System.Collections.Generic;
    using System.Linq;
    using EFT.Response;

    /// <summary>
    ///     (From section 8.9 of the SAS v5.02 document)  -
    ///     https://confy.aristocrat.com/pages/viewpage.action?pageId=159599156
    ///     EFT Long poll parser for handling 0x66 request, Send Last Cashout Credit Amount to the host.
    /// </summary>
    [Sas(SasGroup.Eft)]
    public class
        LP66SendLastCashoutCreditAmountParser : SasLongPollParser<EftSendLastCashoutResponse, EftSendLastCashoutData>
    {
        /// <summary>
        ///     Instantiates a new instance of the LP66SendLastCashoutCreditAmountParser class
        /// </summary>
        public LP66SendLastCashoutCreditAmountParser()
            : base(LongPoll.EftSendLastCashOutCreditAmount)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var requestCommandData =
                new EftSendLastCashoutData
                {
                    Acknowledgement = command.ToList()[2] == 0x01
                };
            var response = Handle(requestCommandData);
            Handlers = response.Handlers;
            return GenerateResponseBytes(response, command);
        }

        private static List<byte> GenerateResponseBytes(
            EftSendLastCashoutResponse response,
            IReadOnlyCollection<byte> command)
        {
            var responseByte = new List<byte>();
            responseByte.AddRange(command.Take(3));
            responseByte.Add((byte)response.Status);
            responseByte.AddRange(Utilities.ToBcd(response.LastCashoutAmount, SasConstants.Bcd10Digits));
            return responseByte;
        }
    }
}