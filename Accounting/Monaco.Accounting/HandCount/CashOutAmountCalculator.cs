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

        public CashOutAmountCalculator()
            :
            this(ServiceManager.GetInstance().GetService<IHandCountService>())
        {

        }

        public CashOutAmountCalculator(IHandCountService handCountService)
        {
            _handCountService = handCountService ?? throw new ArgumentNullException(nameof(handCountService));
        }

        public string Name => typeof(CashOutAmountCalculator).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(ICashOutAmountCalculator) };

        public long GetCashableAmount(long amount)
        {
            return VoucherTicketsCreator.GetMillicentAmount(Math.Min(VoucherTicketsCreator.GetDollarAmount(amount), _handCountService.HandCount * 5));
        }

        public int GetHandCountUsed(long amount)
        {
            return (int)Math.Ceiling(VoucherTicketsCreator.GetDollarAmount(amount) / 5);
        }

        public void Initialize()
        {
            
        }
    }
}
