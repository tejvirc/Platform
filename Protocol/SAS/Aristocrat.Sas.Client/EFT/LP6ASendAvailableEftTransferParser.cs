namespace Aristocrat.Sas.Client.EFT
{
    using System.Collections.Generic;

    [Sas(SasGroup.Eft)]
    public class LP6ASendAvailableEftTransferParser : SasLongPollParser<AvailableEftTransferResponse, LongPollData>
    {
        /// <summary>
        ///     Instantiates a new instance of the LP6ASendAvailableEftTransfers class
        /// </summary>
        public LP6ASendAvailableEftTransferParser()
            : base(LongPoll.EftSendAvailableEftTransfers)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var response = new List<byte>(command);
            var result = Handle(Data);

            response.AddRange(result.Reserved);
            response.AddRange(Utilities.ToBinary((uint)result.TransferAvailability, 1));
            return response;
        }
    }
}