namespace Aristocrat.Monaco.TestProtocol
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;

    public class TestHandpayValidator : IHandpayValidator
    {
        /// <inheritdoc />
        public bool ValidateHandpay(
            long cashableAmount,
            long promoAmount,
            long nonCashAmount,
            HandpayType handpayType)
        {
            return true;
        }

        /// <inheritdoc />
        public Task RequestHandpay(HandpayTransaction transaction)
        {
            // Normally we'd send something to the host here, but there really isn't anything to do
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task HandpayKeyedOff(HandpayTransaction transaction) => Task.CompletedTask;

        /// <inheritdoc />
        public bool AllowLocalHandpay => true;

        /// <inheritdoc />
        public string Name => nameof(TestHandpayValidator);

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IHandpayValidator) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public bool HostOnline => true;

        /// <inheritdoc />
        public bool LogTransactionRequired(ITransaction _)
        {
            return true;
        }
    }
}