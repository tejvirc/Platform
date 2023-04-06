namespace Aristocrat.Monaco.Accounting.HandCount
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Contracts.HandCount;
    using Kernel;
    using Vouchers;

    /// <summary>
    ///     An <see cref="ICashOutAmountCalculator" /> 
    /// </summary>
    public class CashOutAmountCalculator : ICashOutAmountCalculator
    {
        private readonly IHandCountService _handCountService;
        private readonly decimal _cashoutAmountPerHand = 0;

        public CashOutAmountCalculator()
            :
            this(
                ServiceManager.GetInstance().GetService<IHandCountService>(),
                 ServiceManager.GetInstance().GetService<IPropertiesManager>()
                )
        {

        }

        public CashOutAmountCalculator(IHandCountService handCountService, IPropertiesManager properties)
        {
            _handCountService = handCountService ?? throw new ArgumentNullException(nameof(handCountService));
            _cashoutAmountPerHand = VoucherTicketsCreator.GetDollarAmount(properties.GetValue(AccountingConstants.CashoutAmountPerHandCount, 0L));
        }

        public string Name => typeof(CashOutAmountCalculator).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(ICashOutAmountCalculator) };

        public long GetCashableAmount(long amount)
        {
            return VoucherTicketsCreator.GetMillicentAmount(Math.Min(VoucherTicketsCreator.GetDollarAmount(amount), _handCountService.HandCount * _cashoutAmountPerHand));
        }

        public int GetHandCountUsed(long amount)
        {
            return (int)Math.Ceiling(VoucherTicketsCreator.GetDollarAmount(amount) / _cashoutAmountPerHand);
        }

        public void Initialize()
        {

        }
    }
}
