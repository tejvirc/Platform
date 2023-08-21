namespace Aristocrat.Monaco.Bingo.Handpay.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Gaming.Contracts.Central;
    using Gaming.Contracts.Payment;
    using Kernel;

    public class TotalWinsJackpotDeterminationStrategy : IJackpotDeterminationStrategy
    {
        private readonly IPropertiesManager _properties;

        public TotalWinsJackpotDeterminationStrategy(IPropertiesManager properties)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public IEnumerable<PaymentDeterminationResult> GetPaymentResults(
            long winInMillicents,
            CentralTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            var largeWinLimit =
                _properties.GetValue(AccountingConstants.LargeWinLimit, AccountingConstants.DefaultLargeWinLimit);
            var totalOutcomes = transaction.Outcomes.Sum(x => x.Value);
            var paymentResult = Math.Max(winInMillicents, totalOutcomes);
            return new List<PaymentDeterminationResult>
            {
                paymentResult >= largeWinLimit
                    ? new PaymentDeterminationResult(0, paymentResult, Guid.NewGuid())
                    : new PaymentDeterminationResult(paymentResult, 0, Guid.NewGuid())
            };
        }
    }
}