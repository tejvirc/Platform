namespace Aristocrat.Monaco.Bingo.Handpay.Strategies
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Gaming.Contracts.Central;
    using Gaming.Contracts.Payment;
    using Kernel;

    public class InterimPatternJackpotDeterminationStrategy : IJackpotDeterminationStrategy
    {
        private readonly IPropertiesManager _properties;

        public InterimPatternJackpotDeterminationStrategy(IPropertiesManager properties)
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

            var (handpay, credit) = GetPaymentAmounts(transaction);

            return new List<PaymentDeterminationResult>
            {
                new PaymentDeterminationResult(credit, handpay, Guid.NewGuid())
            };
        }

        private (long handpay, long credit) GetPaymentAmounts(CentralTransaction transaction)
        {
            var handpay = 0L;
            var credit = 0L;
            var largeWinLimit =
                _properties.GetValue(AccountingConstants.LargeWinLimit, AccountingConstants.DefaultLargeWinLimit);
            foreach (var outcome in transaction.Outcomes)
            {
                if (outcome.Value >= largeWinLimit)
                {
                    handpay += outcome.Value;
                }
                else
                {
                    credit += outcome.Value;
                }
            }

            return (handpay, credit);
        }
    }
}