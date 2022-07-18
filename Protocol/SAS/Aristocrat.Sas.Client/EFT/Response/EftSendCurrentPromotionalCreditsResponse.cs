namespace Aristocrat.Sas.Client.Eft.Response
{
    /// <summary>
    ///     The Eft send current promotional credit response 
    /// </summary>
    public class EftSendCurrentPromotionalCreditsResponse : LongPollResponse
    {
        /// <summary>
        ///     Initializes a new instance of the EftSendCurrentPromotionalCreditsResponse class.
        /// </summary>
        /// <param name="currentPromoCredits">current promotional credits in cents</param>
        public EftSendCurrentPromotionalCreditsResponse(ulong currentPromoCredits)
        {
            CurrentPromotionalCredits = currentPromoCredits;
        }

        /// <summary>
        ///     Current Promotional Credits in cents
        /// </summary>
        public ulong CurrentPromotionalCredits { get; }
    }
}