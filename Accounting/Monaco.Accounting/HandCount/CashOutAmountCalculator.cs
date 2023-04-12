namespace Aristocrat.Monaco.Accounting.HandCount
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Contracts.HandCount;
    using Kernel;

    /// <summary>
    ///     An <see cref="ICashOutAmountCalculator" /> 
    /// </summary>
    public class CashOutAmountCalculator : ICashOutAmountCalculator
    {
        private readonly IHandCountService _handCountService;
        private readonly long _cashoutAmountPerHand;

        public CashOutAmountCalculator()
            : this(
                ServiceManager.GetInstance().GetService<IHandCountService>(),
                 ServiceManager.GetInstance().GetService<IPropertiesManager>()
                )
        {
        }

        public CashOutAmountCalculator(IHandCountService handCountService, IPropertiesManager properties)
        {
            _handCountService = handCountService ?? throw new ArgumentNullException(nameof(handCountService));
            _cashoutAmountPerHand = properties.GetValue(AccountingConstants.CashoutAmountPerHandCount, 0L);
        }

        public string Name => typeof(CashOutAmountCalculator).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(ICashOutAmountCalculator) };

        public long GetCashableAmount(long amount)
        {
            return Math.Min(amount, _handCountService.HandCount * _cashoutAmountPerHand);
        }

        public int GetHandCountUsed(long amount)
        {
            return (int)Math.Ceiling(amount / (decimal)_cashoutAmountPerHand);
        }

        public void Initialize()
        {
        }
    }
}
