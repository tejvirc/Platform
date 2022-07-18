namespace Aristocrat.Sas.Client.EFT
{
    /// <summary>
    /// Long poll parser for handling 0x69 request.
    /// Message structure defines in SasLongPollTypeDParser
    /// </summary>
    [Sas(SasGroup.Eft)]
    public class LP69TransferCashableCreditParser :  SasLongPollTypeDParser
    {
        /// <summary>
        /// All business logic for this parser is implemented within the Parse method in base class SasLongPollTypeDParser
        /// </summary>
        public LP69TransferCashableCreditParser()
            : base(LongPoll.EftTransferCashableCreditsToMachine) //this parameter indicates LP69 command will be handled in base class
        {
        }
    }
}