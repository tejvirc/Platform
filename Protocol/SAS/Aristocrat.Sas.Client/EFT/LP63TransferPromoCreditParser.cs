namespace Aristocrat.Sas.Client.EFT
{
    /// <summary>
    /// Long poll parser for handling 0x63 request.
    /// </summary>
    [Sas(SasGroup.Eft)]
    public class LP63TransferPromoCreditParser : SasLongPollTypeDParser
    {
        /// <inheritdoc />
        public LP63TransferPromoCreditParser()
            : base(LongPoll.EftTransferPromotionalCreditsToMachine)
        {
        }
    }
}
