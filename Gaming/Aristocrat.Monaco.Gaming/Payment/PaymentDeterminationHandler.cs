namespace Aristocrat.Monaco.Gaming.Payment
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Contracts.Payment;
    using Kernel;

    /// <summary>
    ///     The default implementation of <see cref="IPaymentDeterminationHandler" /> which simply applies the LargeWinLimit
    ///     setting to determine what to do with the amount.
    /// </summary>
    public class PaymentDeterminationHandler : IPaymentDeterminationHandler
    {
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Construct the default <see cref="IPaymentDeterminationHandler" /> implementation using the properties manager
        ///     so we can get the LargeWinLimit setting.
        /// </summary>
        /// <param name="properties">The properties manager for getting system settings.</param>
        public PaymentDeterminationHandler(IPropertiesManager properties)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <inheritdoc />
        public List<PaymentDeterminationResult> GetPaymentResults(long winInMillicents, bool isPayGameResults=true)
        {
            var largeWinLimit =
                _properties.GetValue(AccountingConstants.LargeWinLimit, AccountingConstants.DefaultLargeWinLimit);

            var result = new PaymentDeterminationResult(0, 0, Guid.NewGuid());
            if (winInMillicents >= largeWinLimit)
            {
                result.MillicentsToPayUsingLargeWinStrategy = winInMillicents;
            }
            else
            {
                result.MillicentsToPayToCreditMeter = winInMillicents;
            }

            return new List<PaymentDeterminationResult> { result };
        }
    }
}