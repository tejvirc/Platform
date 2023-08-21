namespace Aristocrat.Monaco.Gaming.Contracts.Payment
{
    using System;
    using Application.Contracts.Extensions;

    /// <summary>
    ///     A class that tells us what parts of a win are to be paid directly to the player or to a handpay, voucher, etc via a
    ///     <see cref="LargeWinCashOutStrategy"/>.
    /// </summary>
    public class PaymentDeterminationResult
    {
        /// <summary>
        ///     The part of the win that is not "large". Normally this is the whole amount if the amount is under the IRS limit.
        ///     For some jurisdictions though, if the win is large then this may contain a partial amount that is to be sent
        ///     to the credit meter.
        /// </summary>
        public long MillicentsToPayToCreditMeter { get; set; }

        /// <summary>
        ///     The part of the win that is "large". Normally this is the whole amount if the amount is over the IRS limit.
        ///     For some jurisdictions though, if the win is large then this may contain a partial amount that is to be sent
        ///     to the handpay or cash out.
        /// </summary>
        public long MillicentsToPayUsingLargeWinStrategy { get; set; }

        /// <summary>
        ///     A unique identifier for this result. Normally this will just be the Guid used for the resulting handpay or
        ///     voucher out operation. For some jurisdictions though we will need this value somewhere so that we can deal
        ///     with the requirements as each individual amount is keyed off - see
        ///     <see cref="Accounting.Contracts.Handpay.HandpayCompletedEvent"/>.
        /// </summary>
        public Guid TransactionIdentifier { get; set; }

        /// <summary>
        ///     Construct a PaymentDeterminationResult with the amount to pay directly to the credit meter and the amount to consider "large".
        /// </summary>
        /// <param name="amountToPayToCreditMeter">The amount that can go directly to the credit meter.</param>
        /// <param name="amountToPayUsingLargeWinStrategy">The amount that must be handpaid or cashed out.</param>
        /// <param name="identifier">The identifier for this result, for later use as a context.</param>
        /// <param name="isCentsNotMillicents">If true, indicates amounts are in cents, otherwise millicents.</param>
        public PaymentDeterminationResult(long amountToPayToCreditMeter, long amountToPayUsingLargeWinStrategy, Guid identifier, bool isCentsNotMillicents = false)
        {
            if (isCentsNotMillicents)
            {
                MillicentsToPayToCreditMeter = amountToPayToCreditMeter.CentsToMillicents();
                MillicentsToPayUsingLargeWinStrategy = amountToPayUsingLargeWinStrategy.CentsToMillicents();
            }
            else
            {
                MillicentsToPayToCreditMeter = amountToPayToCreditMeter;
                MillicentsToPayUsingLargeWinStrategy = amountToPayUsingLargeWinStrategy;
            }

            TransactionIdentifier = identifier;
        }
    }
}
