namespace Aristocrat.Monaco.Gaming.Contracts.Central
{
    using Common;
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Provides a mechanism to interact with a central determinant provider
    /// </summary>
    [CLSCompliant(false)]
    public interface ICentralProvider : IHandlerConnector<ICentralHandler>
    {
        /// <summary>
        ///     Gets the central transactions
        /// </summary>
        IEnumerable<CentralTransaction> Transactions { get; }

        /// <summary>
        ///     Gets one or more outcomes from a central determinant provider
        /// </summary>
        /// <param name="gameId">The unique game Id</param>
        /// <param name="denomination">The selected denomination</param>
        /// <param name="wagerCategory">The associated wager category</param>
        /// <param name="wager">The wager amount</param>
        /// <param name="request">The outcome request</param>
        /// <param name="recovering">true, if the request is occurring during recovery</param>
        /// <returns>true, if the request was successful</returns>
        bool RequestOutcomes(int gameId, long denomination, string wagerCategory, long wager, IOutcomeRequest request, bool recovering);

        /// <summary>
        ///     Called when the outcomes are received from the central determinant host
        /// </summary>
        /// <param name="transactionId">The associated transaction id</param>
        /// <param name="outcomes">The list of outcomes</param>
        /// <param name="exception">The exception, None if successful</param>
        /// <param name="descriptions">The outcome descriptions for this transaction</param>
        void OutcomeResponse(long transactionId, IEnumerable<Outcome> outcomes, OutcomeException exception, IEnumerable<IOutcomeDescription> descriptions);

        /// <summary>
        ///     Sets the <see cref="CentralTransaction" /> using the specified transaction identifier as acknowledged
        /// </summary>
        /// <param name="transactionId">The transaction identifier</param>
        void AcknowledgeOutcome(long transactionId);

        /// <summary>
        ///     Updates the outcome descriptions with the data provided
        /// </summary>
        /// <param name="transactionId">The transaction identifier</param>
        /// <param name="descriptions">The updated description list</param>
        void UpdateOutcomeDescription(long transactionId, IEnumerable<IOutcomeDescription> descriptions);
    }
}