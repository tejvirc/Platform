namespace Aristocrat.Monaco.Accounting.HandCount
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Button;
    using Contracts;
    using Kernel;

    public class HandCountTransferOutExtension : ITransferOutExtension
    {
        private readonly IHandCountService _handCountService;
        private readonly ICashOutAmountCalculator _cashOutAmountCalculator;
        private readonly IEventBus _bus;
        private readonly ISystemDisableManager _systemDisableManager;

        public string Name => typeof(HandCountTransferOutExtension).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(ITransferOutExtension) };

        public HandCountTransferOutExtension()
            : this(ServiceManager.GetInstance().GetService<IHandCountService>(),
        ServiceManager.GetInstance().GetService<ICashOutAmountCalculator>(),
        ServiceManager.GetInstance().GetService<IEventBus>(),
        ServiceManager.GetInstance().GetService<ISystemDisableManager>()
        )
        {
        }

        public HandCountTransferOutExtension(
            IHandCountService handCountService,
            ICashOutAmountCalculator cashOutAmountCalculator,
            IEventBus bus,
            ISystemDisableManager systemDisableManager
            )
        {
            _handCountService = handCountService ?? throw new ArgumentNullException(nameof(handCountService));
            _cashOutAmountCalculator = cashOutAmountCalculator ?? throw new ArgumentNullException(nameof(cashOutAmountCalculator));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _systemDisableManager = systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
        }

        public long PreProcessor(long amount)
        {
            amount = _cashOutAmountCalculator.GetCashableAmount(amount);

            CashOutAsync(amount).Wait();
            return amount;
        }

        private async Task CashOutAsync(long amount)
        {
            if (amount > 2000000)
            {
                var keyOff = Initiate();
                await keyOff.Task;

                _systemDisableManager.Enable(ApplicationConstants.LargePayoutDisableKey);
            }
        }

        private TaskCompletionSource<object> Initiate()
        {
            var keyOff = new TaskCompletionSource<object>();

            _bus.Subscribe<DownEvent>(
                this,
                _ =>
                {
                    keyOff.TrySetResult(null);
                },
                evt => evt.LogicalId == (int)ButtonLogicalId.Button30);

            _systemDisableManager.Disable(
                ApplicationConstants.LargePayoutDisableKey,
                SystemDisablePriority.Immediate,
                () => "COLLECT LIMIT REACHED. SEE ATTENDANT.",
                true,
                () => "COLLECT LIMIT REACHED. SEE ATTENDANT.");

            return keyOff;
        }

        public void PosProcessor(long amount)
        {
            var handCount = _cashOutAmountCalculator.GetHandCountUsed(amount);
            _handCountService.DecreaseHandCount(handCount);

        }

        public void Initialize()
        {
            
        }
    }
}
