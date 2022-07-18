namespace Aristocrat.Monaco.Sas.Eft
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.Eft.Response;
    using Contracts.Eft;

    /// <summary>
    ///     Handles LP27, Request Current Promotional credit. (Section 8.11.1 of the SAS v5.02 document)
    /// </summary>
    public class LP27SendCurrentPromotionalCreditsHandler :
        ISasLongPollHandler<EftSendCurrentPromotionalCreditsResponse, LongPollData>
    {
        private readonly IEftTransferProvider _provider;

        /// <summary>
        ///     Creates and returns a new instance of LP27 handler.
        /// </summary>
        public LP27SendCurrentPromotionalCreditsHandler(IEftTransferProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }
        
        /// <inheritdoc ref="ISasLongPollHandler" />
        public List<LongPoll> Commands => new() { LongPoll.EftSendCurrentPromotionalCredits };

        /// <inheritdoc />
        public EftSendCurrentPromotionalCreditsResponse Handle(LongPollData data)
        {
            var currentPromoCredits = _provider.GetCurrentPromotionalCredits();
            return new EftSendCurrentPromotionalCreditsResponse((ulong)currentPromoCredits);
        }
    }
}