namespace Aristocrat.Sas.Client.EFT
{
    /// <summary>
    ///     Long poll parser for handling 0x6B request.
    /// </summary>
    [Sas(SasGroup.Eft)]
    public class LP6BTransferPromotionalCreditsToHostParser : SasLongPollTypeUParser
    {
        public LP6BTransferPromotionalCreditsToHostParser()
            : base(LongPoll.EftTransferPromotionalCreditsToHost)
        {
        }
    }
}