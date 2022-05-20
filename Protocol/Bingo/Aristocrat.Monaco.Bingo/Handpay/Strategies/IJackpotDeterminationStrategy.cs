namespace Aristocrat.Monaco.Bingo.Handpay.Strategies
{
    using System.Collections.Generic;
    using Gaming.Contracts.Central;
    using Gaming.Contracts.Payment;

    /// <summary>
    ///     A jackpot determination strategy that will provide a set of payment determination results based on the win amount and central transaction
    /// </summary>
    public interface IJackpotDeterminationStrategy
    {
        /// <summary>
        ///     Gets the payment determination results for the provide win amount and central transaction
        /// </summary>
        /// <param name="winInMillicents">The win in millicents to get the results for</param>
        /// <param name="transaction">The central transaction to get the results for</param>
        /// <returns>The collection of payment determination results</returns>
        public IEnumerable<PaymentDeterminationResult> GetPaymentResults(
            long winInMillicents,
            CentralTransaction transaction);
    }
}