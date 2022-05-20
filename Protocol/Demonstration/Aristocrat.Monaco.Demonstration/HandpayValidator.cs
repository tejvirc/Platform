namespace Aristocrat.Monaco.Demonstration
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Kernel;

    public class HandpayValidator : IHandpayValidator, IService
    {
        /// <inheritdoc />
        public bool ValidateHandpay(long cashableAmount, long promoAmount, long nonCashAmount, HandpayType handpayType)
        {
            return true;
        }

        /// <inheritdoc />
        public Task RequestHandpay(HandpayTransaction transaction)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public bool AllowLocalHandpay => true;

        /// <inheritdoc />
        public bool HostOnline => true;

        /// <inheritdoc />
        public bool LogTransactionRequired(ITransaction _)
        {
            return true;
        }

        /// <inheritdoc />
        public string Name => nameof(HandpayValidator);

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IHandpayValidator) };

        /// <inheritdoc />
        public void Initialize()
        {
        }
    }
}