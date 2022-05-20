namespace Aristocrat.Monaco.Bingo.Handpay.Strategies
{
    using System;
    using System.Collections.Generic;
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
            var largeWinLimit =
                _properties.GetValue(AccountingConstants.LargeWinLimit, AccountingConstants.DefaultLargeWinLimit);
            return new List<PaymentDeterminationResult>
            {
                winInMillicents >= largeWinLimit
                    ? new PaymentDeterminationResult(0, winInMillicents, Guid.NewGuid())
                    : new PaymentDeterminationResult(winInMillicents, 0, Guid.NewGuid())
            };
        }
    }
}