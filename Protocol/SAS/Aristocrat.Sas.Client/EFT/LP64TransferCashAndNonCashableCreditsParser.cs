namespace Aristocrat.Sas.Client.EFT
{
    /// <summary>
    /// Long poll parser for handling 0x64 request.
    /// </summary>
    [Sas(SasGroup.Eft)]
    public class LP64TransferCashAndNonCashableCreditsParser : SasLongPollTypeUParser
    {
        /// <inheritdoc />
        public LP64TransferCashAndNonCashableCreditsParser()
            : base(LongPoll.EftTransferCashAndNonCashableCreditsToHost)
        {
        }
    }
}