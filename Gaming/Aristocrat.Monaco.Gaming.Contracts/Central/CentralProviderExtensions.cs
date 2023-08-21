namespace Aristocrat.Monaco.Gaming.Contracts.Central
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Extension methods for the central provider
    /// </summary>
    [CLSCompliant(false)]
    public static class CentralProviderExtensions
    {
        /// <summary>
        ///     Called when the outcomes are received from the central determinant host
        /// </summary>
        /// <param name="provider">The outcome provider to set the response for</param>
        /// <param name="transactionId">The associated transaction id</param>
        /// <param name="outcomes">The list of outcomes</param>
        /// <param name="exception">The exception, None if successful</param>
        /// <param name="gameRoundInfo">The game round info</param>
        public static void OutcomeResponse(
            this ICentralProvider provider,
            long transactionId,
            IEnumerable<Outcome> outcomes,
            OutcomeException exception = OutcomeException.None,
            string gameRoundInfo = "")
        {
            var descriptions = string.IsNullOrEmpty(gameRoundInfo)
                ? Enumerable.Empty<IOutcomeDescription>()
                : new List<IOutcomeDescription> { new TextOutcomeDescription { GameRoundInfo = gameRoundInfo } };

            provider.OutcomeResponse(transactionId, outcomes, exception, descriptions);
        }
    }
}